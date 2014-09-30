﻿using System;
using System.Xml;
using SikkerDigitalPost.Net.KlientApi.Envelope.EnvelopeBody;

namespace SikkerDigitalPost.Net.KlientApi.Envelope.Body
{
    public class StandardBusinessDocument
    {
        private const string Ns3 = "http://www.unece.org/cefact/namespaces/StandardBusinessDocumentHeader";
        private const string Ns5 = "http://www.w3.org/2000/09/xmldsig#";
        private const string Ns9 = "http://begrep.difi.no/sdp/schema_v10";
        
        private XmlDocument _dokument;
        
        public StandardBusinessDocument(XmlDocument dokument)
        {
            _dokument = dokument;
        }

        public XmlElement Xml()
        {
            var sbdElement = _dokument.CreateElement("ns3", "StandardBusinessDocument", Ns3);
            sbdElement.SetAttribute("xmlns:ns3", Ns3);
            sbdElement.SetAttribute("xmlns:ns5", Ns5);
            sbdElement.SetAttribute("xmlns:ns9", Ns9);

            var sbdHeader = new StandardBusinessDocumentHeader(_dokument);
            sbdElement.AppendChild(sbdHeader.Xml());

            var digitalPost = new DigitalPost(_dokument);
            sbdElement.AppendChild(digitalPost.Xml());

            return sbdElement;
        }
    }
}
