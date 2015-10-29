﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using ApiClientShared;
using Difi.SikkerDigitalPost.Klient.Security;
using Difi.SikkerDigitalPost.Klient.Tester.testdata.meldinger;
using Difi.SikkerDigitalPost.Klient.Utilities;

namespace Difi.SikkerDigitalPost.Klient.XmlValidering.Tests
{
    [TestClass()]
    public class ResponsvalidatorTester
    {
        //Send melding
        private readonly string _sendtMeldingXmlTestMiljø = SendtMelding.FunksjoneltTestMiljø;
        private readonly string _responsTransportkvitteringXmlTestmiljø = TransportKvittering.TransportOkKvittertingFunksjoneltTestmiljø;

        //Hent kvittering
        private readonly string _sendtKvitteringsforespørsel = Kvitteringsforespørsel.FunksjoneltTestmiljø ;
        private readonly string _responsKvitteringsForespørsel = KvitteringsRespons.FunksjoneltTestmiljø;

        private readonly string _tomKøKvittering = KvitteringsRespons.TomKøResponsFunksjoneltTestmiljø;
        
        private static XmlDocument TilXmlDokument(string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            return xmlDocument;
        }

        private static void AddRsaSha256AlgorithmToCryptoConfig()
        {
            const string signatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            
            if (CryptoConfig.CreateFromName(signatureMethod) == null)
                CryptoConfig.AddAlgorithm(typeof(RsaPkCs1Sha256SignatureDescription), signatureMethod);
        }


        [TestClass]
        public class KonstruktørMethod : ResponsvalidatorTester
        {
            [TestMethod]
            public void EnkelKonstruktør()
            {
                ////Arrange
                var sendtMelding = TilXmlDokument(_sendtMeldingXmlTestMiljø);
                var respons = TilXmlDokument(_responsTransportkvitteringXmlTestmiljø);
                var miljø = Miljø.FunksjoneltTestmiljø;
                Responsvalidator responsvalidator = new Responsvalidator(respons, sendtMelding, miljø.Sertifikatvalidator);

                //Act

                //Assert
                Assert.AreEqual(miljø.Sertifikatvalidator, responsvalidator.Sertifikatvalidator);
                Assert.AreEqual(sendtMelding, responsvalidator.SendtMelding);
                Assert.AreEqual(respons, responsvalidator.Respons);
            }
        }

        [TestClass]
        public class ValiderTransportkvitteringMethod : ResponsvalidatorTester
        {
            [TestMethod]
            public void TestsertifikatValiderer()
            {
                //Arrange
                AddRsaSha256AlgorithmToCryptoConfig();

                var miljø = Miljø.FunksjoneltTestmiljø;
                var sendtMeldingXmlDocument = TilXmlDokument(_sendtMeldingXmlTestMiljø);
                Responsvalidator responsvalidator = new Responsvalidator(TilXmlDokument(_responsTransportkvitteringXmlTestmiljø), sendtMeldingXmlDocument, miljø.Sertifikatvalidator);
                GuidUtility guidUtility = new GuidUtility()
                {
                    BinarySecurityTokenId = "X509-513ffecb-cd7e-4bb3-a4c5-47eff314683f",
                    BodyId = "soapBody",
                    DokumentpakkeId = "4fa27c07-8a0f-45a9-954e-c658f6c480af@meldingsformidler.sdp.difi.no",
                    EbMessagingId = "id-68ae7123-bf5c-4d15-835c-4a6b91106e77",
                    StandardBusinessDocumentHeaderId = "388214db-29cc-43c7-9543-877e017e5bb4",
                    TimestampId = "TS-76740c34-88d2-4bb6-82d2-9e9f0e474087"
                };

                //Act
                responsvalidator.ValiderTransportkvittering(guidUtility);
                
                //Assert
            }

            [TestMethod]
            public void KrasjerVedUtbyttetDokuemtpakkeId()
            {
                //Arrange
                AddRsaSha256AlgorithmToCryptoConfig();

                var miljø = Miljø.FunksjoneltTestmiljø;
                var sendtMeldingXmlDocument = TilXmlDokument(_sendtMeldingXmlTestMiljø);
                var transportkvitteringXmlDokument = TilXmlDokument(TransportKvittering.TransportOkKvitteringMedByttetDokumentpakkeIdFunksjoneltTestmiljø);
                Responsvalidator responsvalidator = new Responsvalidator(transportkvitteringXmlDokument, sendtMeldingXmlDocument, miljø.Sertifikatvalidator);
                GuidUtility guidUtility = new GuidUtility()
                {
                    BinarySecurityTokenId = "X509-513ffecb-cd7e-4bb3-a4c5-47eff314683f",
                    BodyId = "soapBody",
                    DokumentpakkeId = "4fa27c07-8a0f-45a9-954e-c658f6c480af@meldingsformidler.sdp.difi.no",
                    EbMessagingId = "id-68ae7123-bf5c-4d15-835c-4a6b91106e77",
                    StandardBusinessDocumentHeaderId = "388214db-29cc-43c7-9543-877e017e5bb4",
                    TimestampId = "TS-76740c34-88d2-4bb6-82d2-9e9f0e474087"
                };

                //Act
                responsvalidator.ValiderTransportkvittering(guidUtility);

                //Assert
                Assert.Fail();
            }

        }


        [TestClass]
        public class ValiderMeldingsKvitteringMethod : ResponsvalidatorTester
        {
            [TestMethod]
            public void TestsertifikatValiderer()
            {
                //Arrange
                AddRsaSha256AlgorithmToCryptoConfig();
                var miljø = Miljø.FunksjoneltTestmiljø;

                XmlDocument sendtKvitteringsForespørsel = new XmlDocument();
                sendtKvitteringsForespørsel.LoadXml(_sendtKvitteringsforespørsel);
                Responsvalidator responsvalidator = new Responsvalidator(respons: TilXmlDokument(_responsKvitteringsForespørsel), sendtMelding: sendtKvitteringsForespørsel, sertifikatvalidator: miljø.Sertifikatvalidator);

                //Act
                responsvalidator.ValiderMeldingskvittering();

                //Assert
            }
        }

        [TestClass]
        public class ValiderTomKøKvitteringMethod : ResponsvalidatorTester
        {
            [TestMethod]
            public void TestsertifikatValiderer()
            {
                //Arrange
                AddRsaSha256AlgorithmToCryptoConfig();

                var miljø = Miljø.FunksjoneltTestmiljø;
                XmlDocument sendtKvitteringsForespørsel = new XmlDocument();
                sendtKvitteringsForespørsel.LoadXml(_sendtKvitteringsforespørsel);
                Responsvalidator responsvalidator = new Responsvalidator(TilXmlDokument(_tomKøKvittering), sendtKvitteringsForespørsel, miljø.Sertifikatvalidator);
                
                //Act
                responsvalidator.ValiderTomkøkvittering();

                //Assert
            }
        }
    }
}