﻿using System;
using System.Xml;
using Difi.SikkerDigitalPost.Klient.Extensions;

namespace Difi.SikkerDigitalPost.Klient.Domene.Entiteter.Kvitteringer.Forretning
{
    /// <summary>
    /// Dette er Kvittering på at posten har kommet i retur og har blitt makulert.
    /// Les mer på http://begrep.difi.no/SikkerDigitalPost/1.2.0/meldinger/ReturpostKvittering
    /// </summary>
    public class Returpostkvittering : Forretningskvittering
    {
        public DateTime Returnert { get { return Generert; } }

        public Returpostkvittering(Guid konversasjonsId, string bodyReferenceUri, string digestValue) : base(konversasjonsId, bodyReferenceUri, digestValue)
        {
        }

        public override string ToString()
        {
            return string.Format("Returnert: {0}, {1}", Returnert.ToStringWithUtcOffset(), base.ToString());
        }
    }

}

