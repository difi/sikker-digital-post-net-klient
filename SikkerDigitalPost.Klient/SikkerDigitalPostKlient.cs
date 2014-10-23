﻿using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using SikkerDigitalPost.Domene.Entiteter.Aktører;
using SikkerDigitalPost.Domene.Entiteter.Kvitteringer;
using SikkerDigitalPost.Domene.Entiteter.Post;
using SikkerDigitalPost.Domene.Exceptions;
using SikkerDigitalPost.Klient.AsicE;
using SikkerDigitalPost.Klient.Envelope;
using SikkerDigitalPost.Klient.Security;
using SikkerDigitalPost.Klient.Utilities;
using SikkerDigitalPost.Klient.XmlValidering;
using System.Diagnostics;

namespace SikkerDigitalPost.Klient
{
    public class SikkerDigitalPostKlient
    {
        private readonly Databehandler _databehandler;
        private readonly Klientkonfigurasjon _konfigurasjon;

        /// <param name="databehandler">
        /// Teknisk avsender er den parten som har ansvarlig for den tekniske utførelsen av sendingen.
        /// Teknisk avsender er den aktøren som står for utførelsen av den tekniske sendingen. 
        /// Hvis sendingen utføres av en databehandler vil dette være databehandleren. 
        /// Hvis sendingen utføres av behandlingsansvarlige selv er dette den behandlingsansvarlige.
        /// </param>
        /// <remarks>
        /// Se <a href="http://begrep.difi.no/SikkerDigitalPost/forretningslag/Aktorer">oversikt over aktører</a>
        /// </remarks>
        public SikkerDigitalPostKlient(Databehandler databehandler)
            : this(databehandler, new Klientkonfigurasjon())
        {

        }

        /// <param name="databehandler">
        /// Teknisk avsender er den parten som har ansvarlig for den tekniske utførelsen av sendingen.
        /// Teknisk avsender er den aktøren som står for utførelsen av den tekniske sendingen. 
        /// Hvis sendingen utføres av en databehandler vil dette være databehandleren. 
        /// Hvis sendingen utføres av behandlingsansvarlige selv er dette den behandlingsansvarlige.
        /// </param>
        /// <param name="konfigurasjon">Klientkonfigurasjon for klienten. Brukes for å sette parametere
        /// som proxy, timeout og URI til meldingsformidler. For å bruke standardkonfigurasjon, lag
        /// SikkerDigitalPostKlient uten Klientkonfigurasjon som parameter.</param>
        /// <remarks>
        /// Se <a href="http://begrep.difi.no/SikkerDigitalPost/forretningslag/Aktorer">oversikt over aktører</a>
        /// </remarks>
        public SikkerDigitalPostKlient(Databehandler databehandler, Klientkonfigurasjon konfigurasjon)
        {
            _databehandler = databehandler;
            _konfigurasjon = konfigurasjon;
            Logging.Initialize(konfigurasjon);
        }

        /// <summary>
        /// Sender en forsendelse til meldingsformidler. Dersom noe feilet i sendingen til meldingsformidler, vil det kastes en exception.
        /// </summary>
        /// <param name="forsendelse">Et objekt som har all informasjon klar til å kunne sendes (mottakerinformasjon, sertifikater, vedlegg mm), enten digitalt eller fyisk.</param>
        public Transportkvittering Send(Forsendelse forsendelse)
        {
            Logging.Log(TraceEventType.Verbose, forsendelse.KonversasjonsId, "Sender ny forsendelse til meldingsformidler.");

            var guidHandler = new GuidHandler();
            var arkiv = new AsicEArkiv(forsendelse, guidHandler, _databehandler.Sertifikat);

            var forretningsmeldingEnvelope = new ForretningsmeldingEnvelope(new EnvelopeSettings(forsendelse, arkiv, _databehandler, guidHandler));

            Logging.Log(TraceEventType.Verbose, forsendelse.KonversasjonsId, "Evelope for forsendelse\r\n" + forretningsmeldingEnvelope.Xml().OuterXml);

            try
            {
                var validering = new ForretningsmeldingEnvelopeValidering();
                var validert = validering.ValiderDokumentMotXsd(forretningsmeldingEnvelope.Xml().OuterXml);
                if (!validert)
                    throw new Exception(validering.ValideringsVarsler);

                var mValidering = new ManifestValidering();
                var mValidert = mValidering.ValiderDokumentMotXsd(arkiv.Manifest.Xml().OuterXml);
                if (!mValidert)
                    throw new Exception(mValidering.ValideringsVarsler);

                var sValidering = new SignaturValidering();
                var sValidert = sValidering.ValiderDokumentMotXsd(arkiv.Signatur.Xml().OuterXml);
                if (!sValidert)
                    throw new Exception(sValidering.ValideringsVarsler);
            }
            catch (Exception e)
            {
                throw new Exception("Envelope xml validerer ikke mot xsd:\n" + e.Message);
            }

            var soapContainer = new SoapContainer { Envelope = forretningsmeldingEnvelope, Action = "\"\"" };
            soapContainer.Vedlegg.Add(arkiv);

            var response = SendSoapContainer(soapContainer);

            Logging.Log(TraceEventType.Verbose, forsendelse.KonversasjonsId, "Kvittering for forsendelse\r\n" + response);

            FileUtility.WriteXmlToFileInBasePath(forretningsmeldingEnvelope.Xml().OuterXml, "Forretningsmelding.xml");
            FileUtility.WriteXmlToFileInBasePath(response, "ForrigeKvittering.xml");

            if (!ValiderSignatur(response))
                throw new SendException("Signatur av respons fra Meldingsformidler var ikke gyldig.");

            if (!ValiderDigests(response, forretningsmeldingEnvelope.Xml(), guidHandler))
                throw new SendException("Hash av body og/eller dokumentpakke er ikke lik for sendte og mottatte dokumenter.");

            return KvitteringFactory.GetTransportkvittering(response);
        }

        /// <summary>
        /// Forespør kvittering for forsendelser. Kvitteringer blir tilgjengeliggjort etterhvert som de er klare i meldingsformidler.
        /// Det er ikke mulig å etterspørre kvittering for en spesifikk forsendelse.
        /// </summary>
        /// <param name="kvitteringsforespørsel"></param>
        /// <returns></returns>
        /// <remarks>
        /// <list type="table">
        /// <listheader><description>Dersom det ikke er tilgjengelige kvitteringer skal det ventes følgende tidsintervaller før en ny forespørsel gjøres</description></listheader>
        /// <item><term>normal</term><description>Minimum 10 minutter</description></item>
        /// <item><term>prioritert</term><description>Minimum 1 minutt</description></item>
        /// </list>
        /// </remarks>
        public Forretningskvittering HentKvittering(Kvitteringsforespørsel kvitteringsforespørsel)
        {
            return HentKvitteringOgBekreftForrige(kvitteringsforespørsel, null);
        }

        /// <summary>
        /// Forespør kvittering for forsendelser med mulighet til å samtidig bekrefte på forrige kvittering for å slippe å kjøre eget kall for bekreft. 
        /// Kvitteringer blir tilgjengeliggjort etterhvert som de er klare i meldingsformidler. Det er ikke mulig å etterspørre kvittering for en 
        /// spesifikk forsendelse. 
        /// </summary>
        /// <param name="kvitteringsforespørsel"></param>
        /// <param name="forrigeKvittering"></param>
        /// <returns></returns>
        /// <remarks>
        /// <list type="table">
        /// <listheader><description>Dersom det ikke er tilgjengelige kvitteringer skal det ventes følgende tidsintervaller før en ny forespørsel gjøres</description></listheader>
        /// <item><term>normal</term><description>Minimum 10 minutter</description></item>
        /// <item><term>prioritert</term><description>Minimum 1 minutt</description></item>
        /// </list>
        /// </remarks>
        public Forretningskvittering HentKvitteringOgBekreftForrige(Kvitteringsforespørsel kvitteringsforespørsel, Forretningskvittering forrigeKvittering)
        {
            if (forrigeKvittering != null)
            {
                Bekreft(forrigeKvittering);
            }

            Logging.Log(TraceEventType.Verbose, "Henter kvittering for " + kvitteringsforespørsel.Mpc);

            var envelopeSettings = new EnvelopeSettings(kvitteringsforespørsel, _databehandler, new GuidHandler());
            var kvitteringsenvelope = new KvitteringsEnvelope(envelopeSettings);

            Logging.Log(TraceEventType.Verbose, "Envelope for Kvitteringsforespørsel\r\n" + kvitteringsenvelope.Xml().OuterXml);

            var soapContainer = new SoapContainer { Envelope = kvitteringsenvelope, Action = "\"\"" };
            
            var kvittering = SendSoapContainer(soapContainer);

            Logging.Log(TraceEventType.Verbose, "Envelope for kvitteringssvar\r\n" + kvittering);

            FileUtility.WriteXmlToFileInBasePath(kvitteringsenvelope.Xml().InnerXml, "Kvitteringsforespørsel.xml");
            FileUtility.WriteXmlToFileInBasePath(kvittering, "Kvittering.xml");

            return KvitteringFactory.GetForretningskvittering(kvittering);
        }

        /// <summary>
        /// Bekreft mottak av forretningskvittering gjennom <see cref="HentKvittering(Kvitteringsforespørsel)"/>.
        /// <list type="bullet">
        /// <listheader><description><para>Dette legger opp til følgende arbeidsflyt</para></description></listheader>
        /// <item><description><para><see cref="HentKvittering(Kvitteringsforespørsel)"/></para></description></item>
        /// <item><description><para>Gjør intern prosessering av kvitteringen (lagre til database, og så videre)</para></description></item>
        /// <item><description><para>Bekreft mottak av kvittering</para></description></item>
        /// </list>
        /// </summary>
        /// <param name="forrigeKvittering"></param>
        /// <remarks>
        /// <see cref="HentKvittering(Kvitteringsforespørsel)"/> kommer ikke til å returnere en ny kvittering før mottak av den forrige er bekreftet.
        /// </remarks>
        public void Bekreft(Forretningskvittering forrigeKvittering)
        {
            var envelopeSettings = new EnvelopeSettings(forrigeKvittering, _databehandler, new GuidHandler());
            var kvitteringMottattEnvelope = new KvitteringMottattEnvelope(envelopeSettings);
            FileUtility.WriteXmlToFileInBasePath(kvitteringMottattEnvelope.Xml().OuterXml, "kvitteringMottattEnvelope.xml");

            Logging.Log(TraceEventType.Verbose, "Envelope for bekreftelse av kvittering\r\n" + kvitteringMottattEnvelope.Xml().OuterXml);

            var soapContainer = new SoapContainer { Envelope = kvitteringMottattEnvelope, Action = "\"\"" };
            var response = SendSoapContainer(soapContainer);

            Logging.Log(TraceEventType.Verbose, "Svar på bekreftelse av kvittering\r\n" + response);
        }

        private string SendSoapContainer(SoapContainer soapContainer)
        {
            string data = String.Empty;

            var request = (HttpWebRequest)WebRequest.Create("https://qaoffentlig.meldingsformidler.digipost.no/api/ebms");

            soapContainer.Send(request);
            try
            {
                var response = request.GetResponse();
                data = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException we)
            {
                using (var response = we.Response as HttpWebResponse)
                {
                    using (Stream errorStream = response.GetResponseStream())
                    {
                        XDocument soap = XDocument.Load(errorStream);
                        var errorFileName = String.Format("{0} - SendSoapContainerFeilet.xml", DateUtility.DateForFile());
                        FileUtility.WriteXmlToFileInBasePath(soap.ToString(), "FeilVedSending", errorFileName);
                        data = soap.ToString();
                    }

                }
            }
            return data;
        }

        private bool ValiderSignatur(string response)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(response);
            XmlNode responseRot = document.DocumentElement;
            var responseMgr = new XmlNamespaceManager(document.NameTable);
            responseMgr.AddNamespace("env", Navnerom.env);
            responseMgr.AddNamespace("ds", Navnerom.ds);

            try
            {
                var signatureNode = (XmlElement)responseRot.SelectSingleNode("//ds:Signature", responseMgr);
                var signed = new SignedXmlWithAgnosticId(document);
                signed.LoadXml(signatureNode);
                return signed.CheckSignature();
            }
            catch (Exception e)
            {
                throw new Exception("Feil under validering av signatur.", e);
            }
        }

        private bool ValiderDigests(string response, XmlDocument envelope, GuidHandler guidHandler)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(response);

            XmlNode responseRot = document.DocumentElement;
            XmlNamespaceManager responseMgr = new XmlNamespaceManager(document.NameTable);
            responseMgr.AddNamespace("env", Navnerom.env);
            responseMgr.AddNamespace("ns5", Navnerom.Ns5);

            try
            {
                var responseBodyDigest = responseRot.SelectSingleNode("//ns5:Reference[@URI = '#" + guidHandler.BodyId + "']", responseMgr).InnerText;
                var responseAsicDigest = responseRot.SelectSingleNode("//ns5:Reference[@URI = 'cid:" + guidHandler.DokumentpakkeId + "']", responseMgr).InnerText;

                var envelopeRot = envelope.DocumentElement;
                var envelopeMgr = new XmlNamespaceManager(envelope.NameTable);
                envelopeMgr.AddNamespace("env", Navnerom.env);
                envelopeMgr.AddNamespace("wsse", Navnerom.wsse);
                envelopeMgr.AddNamespace(String.Empty, Navnerom.Ns5);

                var envelopeBodyDigest = envelopeRot.SelectSingleNode("//*[namespace-uri()='" + Navnerom.ds + "' and local-name()='Reference'][@URI = '#" + guidHandler.BodyId + "']", envelopeMgr).InnerText;
                var envelopeAsicDigest = envelopeRot.SelectSingleNode("//*[namespace-uri()='" + Navnerom.ds + "' and local-name()='Reference'][@URI = 'cid:" + guidHandler.DokumentpakkeId + "']", envelopeMgr).InnerText;

                return responseBodyDigest.Equals(envelopeBodyDigest) && responseAsicDigest.Equals(envelopeAsicDigest);
            }
            catch (Exception e)
            {
                throw new Exception("En feil", e);
            }
        }
    }
}
