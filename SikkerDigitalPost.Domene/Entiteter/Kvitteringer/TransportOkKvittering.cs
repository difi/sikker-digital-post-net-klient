﻿using System;
using System.Xml;

namespace SikkerDigitalPost.Domene.Entiteter.Kvitteringer
{
    /// <summary>
    /// Kvittering fra meldingsformidler som indikerer at denne har overtatt ansvaret for videre formidling av meldingen.
    /// </summary>
    public class TransportOkKvittering : Transportkvittering
    {
        public TransportOkKvittering(XmlDocument document, XmlNamespaceManager namespaceManager) : base(document, namespaceManager)
        {
        }

        public override string ToString()
        {
            return String.Format("{0} med meldingsId {1}:\nTidspunkt: {2}.\nRefererer til melding med id: {3}",
                GetType().Name, MeldingsId, Tidspunkt, ReferanseTilMeldingsId);
        }
    }
}
