﻿using System;
using System.Xml;
using Difi.SikkerDigitalPost.Klient.Domene.Enums;
using Difi.SikkerDigitalPost.Klient.Domene.Exceptions;

namespace Difi.SikkerDigitalPost.Klient.Domene.Entiteter.Kvitteringer.Forretning
{
    /// <summary>
    /// Sendes fra Postkasse til Avsender dersom Postkasse opplever problemer med å utføre varslingen som spesifisert i meldingen.
    /// Les mer på http://begrep.difi.no/SikkerDigitalPost/1.0.2/meldinger/VarslingfeiletKvittering.
    /// </summary>
    public class VarslingFeiletKvittering : Forretningskvittering
    {
        /// <summary>
        /// Kanal for varsling til eier av postkasse. Varsling og påminnelsesmeldinger skal sendes på den kanal som blir spesifisert. Kanalen SMS er priset.
        /// </summary>
        public Varslingskanal Varslingskanal { get; protected set; }

        /// <summary>
        /// Beskrivelse av varsling feilet.
        /// </summary>
        public string Beskrivelse { get; protected set; }
        public VarslingFeiletKvittering() { }
        internal VarslingFeiletKvittering(XmlDocument document, XmlNamespaceManager namespaceManager) : base(document, namespaceManager)
        {
            try
            {
                var varslingskanal = DocumentNode("//ns9:varslingskanal").InnerText;
                Varslingskanal = varslingskanal == Varslingskanal.Epost.ToString()
                    ? Varslingskanal.Epost
                    : Varslingskanal.Sms;

                var beskrivelseNode = DocumentNode("//ns9:beskrivelse");
                if (beskrivelseNode != null)
                    Beskrivelse = beskrivelseNode.InnerText;
            } catch (Exception e)
            {
                throw new XmlParseException(
                    "Feil under bygging av VarslingFeilet-kvittering. Klarte ikke finne alle felter i xml.", e);
            }
        }

        public DateTime Feilet
        {
            get { return Generert; }
        }

        public override string ToString()
        {
            return String.Format("{0} med meldingsId {1}: \nFeilet: {2}. \nVarslingskanal: {3}. \nBeskrivelse: {4}. \nKonversasjonsId: {5}. \nRefererer til melding med id: {6}", 
                GetType().Name, MeldingsId, Feilet, Varslingskanal, Beskrivelse, KonversasjonsId, ReferanseTilMeldingId);
        }
    }
}
