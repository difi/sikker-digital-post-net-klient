﻿using System;
using System.Xml;
using ApiClientShared;
using Difi.SikkerDigitalPost.Klient.AsicE;
using Difi.SikkerDigitalPost.Klient.Domene.Entiteter.Post;
using Difi.SikkerDigitalPost.Klient.Domene.Enums;
using Difi.SikkerDigitalPost.Klient.Tester.Utilities;
using Difi.SikkerDigitalPost.Klient.Utilities;
using Difi.SikkerDigitalPost.Klient.XmlValidering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Difi.SikkerDigitalPost.Klient.Tester
{
    [TestClass]
    public class ManifestTester
    {

        [TestClass]
        public class KonstruktørMethod : ManifestTester
        {
            [TestMethod]
            public void EnkelKonstruktør()
            {
                //Arrange
                const string id = "Id_1";
                const string mimeType = "application/xml";
                const string filnavn = "manifest.xml";

                var forsendelse = DomeneUtility.GetDigitalForsendelseEnkelMedTestSertifikat();
                var manifest = new Manifest(forsendelse);
                
                //Act

                //Assert
                Assert.AreEqual(forsendelse,manifest.Forsendelse);
                Assert.AreEqual(forsendelse.Avsender,manifest.Avsender);
                Assert.AreEqual(id,manifest.Id);
                Assert.AreEqual(mimeType,manifest.MimeType);
                Assert.AreEqual(filnavn,manifest.Filnavn);
            } 
        }


        [TestClass]
        public class Hoveddokument : ManifestTester
        {
            [TestMethod]
            public void UgyldigNavnPåHoveddokumentValidererIkke()
            {
                var arkiv = DomeneUtility.GetAsicEArkivEnkelMedTestSertifikat();

                var manifestXml = arkiv.Manifest.Xml();
                var manifestValidator = new ManifestValidator();

                //Endre navn på hoveddokument til å være for kort
                var namespaceManager = new XmlNamespaceManager(manifestXml.NameTable);
                namespaceManager.AddNamespace("ns9", NavneromUtility.DifiSdpSchema10);
                namespaceManager.AddNamespace("ds", NavneromUtility.XmlDsig);

                var hoveddokumentNode = manifestXml.DocumentElement.SelectSingleNode("//ns9:hoveddokument",
                    namespaceManager);
                var gammelVerdi = hoveddokumentNode.Attributes["href"].Value;
                hoveddokumentNode.Attributes["href"].Value = "abc";

                var validert = manifestValidator.ValiderDokumentMotXsd(manifestXml.OuterXml);
                Assert.IsFalse(validert, manifestValidator.ValideringsVarsler);

                hoveddokumentNode.Attributes["href"].Value = gammelVerdi;
            }
        }

        [TestClass]
        public class Vedlegg : ManifestTester
        {
            [TestMethod]
            public void VedleggTittelSkalSettesIManifestet()
            {
                //Arrange
                //Arrange
                ResourceUtility resourceUtility = new ResourceUtility("Difi.SikkerDigitalPost.Klient.Tester.testdata");
                var dokument = new Dokument("hoved", resourceUtility.ReadAllBytes(true, "hoveddokument", "Hoveddokument.pdf"), "application/pdf");
                var vedleggTittel = "tittel";
                var vedlegg = new Dokument(vedleggTittel, resourceUtility.ReadAllBytes(true, "hoveddokument", "Hoveddokument.pdf"),
                    "application/pdf");


                var dokumentPakke = new Dokumentpakke(dokument);
                dokumentPakke.LeggTilVedlegg(vedlegg);

                var enkelForsendelse = new Forsendelse(DomeneUtility.GetAvsender(), DomeneUtility.GetDigitalPostInfoEnkelMedTestSertifikat(), dokumentPakke, Prioritet.Normal, mpcId: Guid.NewGuid().ToString());
                var asiceArkiv = DomeneUtility.GetAsicEArkiv(enkelForsendelse);

                var manifestXml = asiceArkiv.Manifest.Xml();
                var namespaceManager = new XmlNamespaceManager(manifestXml.NameTable);
                namespaceManager.AddNamespace("ns9", NavneromUtility.DifiSdpSchema10);
                namespaceManager.AddNamespace("ds", NavneromUtility.XmlDsig);
                //Act

                //Assert
                
                var vedleggNodeInnerText = manifestXml.DocumentElement.SelectSingleNode("//ns9:vedlegg",
                    namespaceManager).InnerText;
                Assert.AreEqual(vedleggTittel,vedleggNodeInnerText);
            }

            [TestMethod]
            public void HoveddokumentTittelSkalSettesIManifestet()
            {
                //Arrange
                //Arrange
                ResourceUtility resourceUtility = new ResourceUtility("Difi.SikkerDigitalPost.Klient.Tester.testdata");
                const string hoveddokumentTittel = "hoveddokument tittel";
                var dokument = new Dokument(hoveddokumentTittel, resourceUtility.ReadAllBytes(true, "hoveddokument", "Hoveddokument.pdf"), "application/pdf");
                
                var vedlegg = new Dokument("vedlegg tittel", resourceUtility.ReadAllBytes(true, "hoveddokument", "Hoveddokument.pdf"),
                    "application/pdf");


                var dokumentPakke = new Dokumentpakke(dokument);
                dokumentPakke.LeggTilVedlegg(vedlegg);

                var enkelForsendelse = new Forsendelse(DomeneUtility.GetAvsender(), DomeneUtility.GetDigitalPostInfoEnkelMedTestSertifikat(), dokumentPakke, Prioritet.Normal, mpcId: Guid.NewGuid().ToString());
                var asiceArkiv = DomeneUtility.GetAsicEArkiv(enkelForsendelse);

                var manifestXml = asiceArkiv.Manifest.Xml();
                var namespaceManager = new XmlNamespaceManager(manifestXml.NameTable);
                namespaceManager.AddNamespace("ns9", NavneromUtility.DifiSdpSchema10);
                namespaceManager.AddNamespace("ds", NavneromUtility.XmlDsig);
                //Act

                //Assert

                var vedleggNodeInnerText = manifestXml.DocumentElement.SelectSingleNode("//ns9:hoveddokument",
                    namespaceManager).InnerText;
                Assert.AreEqual(hoveddokumentTittel, vedleggNodeInnerText);
            }
        }

        [TestClass]
        public class XsdValidering
        {
            [TestMethod]
            public void ValidereManifestMotXsdValiderer()
            {
                var arkiv = DomeneUtility.GetAsicEArkivEnkelMedTestSertifikat();

                var manifestXml = arkiv.Manifest.Xml();

                var manifestValidering = new ManifestValidator();
                var validert = manifestValidering.ValiderDokumentMotXsd(manifestXml.OuterXml);
                Assert.IsTrue(validert, manifestValidering.ValideringsVarsler);
            }
        }
    }
}