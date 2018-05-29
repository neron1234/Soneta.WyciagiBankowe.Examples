using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Soneta.Kasa;
using Soneta.WyciagiBankowe.Core;
using Soneta.WyciagiBankowe.Filters.Standard.XML;

//rejestracja filtra
[assembly: ImportWyciągówBankowychFilter(
    "Custom (XML)",
    typeof(CustomXML))]

namespace Soneta.WyciagiBankowe.Filters.Standard.XML
{
    public sealed class CustomXML : FilterBaseXML<CustomXML.XML_WYCIAGI, CustomXML.XML_OPERACJA>
    {
        public override Version Version => new Version(1, 0); //wersja filtra

        //ustawienie obsługiwanych możliwości (bez daty w nagłówku, z datą opcjonalną)
        protected override FeatureSupport Features => FeatureSetOperationDate(base.Features & ~FeatureSupport.HeaderDate, FeatureSupport.OperationDtxOpt);

        public override Encoding GetEncoding() => Encoding.GetEncoding(encodingEurIso); //ustawienie kodowania pliku

        public CustomXML()
        {
            models.Add(new ModelXMLSingle<XML_WYCIAGI>("NAME", "WYCIAGI")); //dodanie modelu danych
        }

        protected override void extractHeader(XML_WYCIAGI mRoot, ReportHeader reportHeader) //pobieranie nagłówka
        {
            reportHeader.AbstractIdent = mRoot.DataOd + "..." + mRoot.DataDo; //identyfikator nagłówka
            reportHeader.AccountNumber = mRoot.Rachunek; //numer rachunku
            reportHeader.CurrencySymbol = mRoot.Waluta; //waluta
        }

        protected override IEnumerable<XML_OPERACJA> extractHeaderOps(XML_WYCIAGI mRoot) => mRoot.Operacje; //wskazanie gdzie w modelu są operacje

        protected override void extractOperation(XML_OPERACJA mOperation, ReportItem reportItem)
        {
            //uzupełnienie obiektu raportu
            reportItem.BookingDate = mOperation.DataKsiegowania.Length == 0
                ? Helpers.Dates.FromYMD(mOperation.DataPlatnosci)
                : Helpers.Dates.FromYMD(mOperation.DataKsiegowania); 
            reportItem.OperationDate = Helpers.Dates.EmpFromYMD(mOperation.DataPlatnosci);
            reportItem.FldAccount = mOperation.Rachunek;
            reportItem.CurrencySymbol = mOperation.Waluta;
            reportItem.AmountDec = Helpers.Numeric.ParseDecimalWithDirection(mOperation.Kwota, mOperation.Strona, strona => Helpers.Numeric.GetDirection(strona, Helpers.Numeric.StrMa, Helpers.Numeric.StrWn));
            reportItem.FldCustomer = Helpers.Str.AppendNL(mOperation.Nazwa1, mOperation.Nazwa2, mOperation.Nazwa3, mOperation.Nazwa4);
            reportItem.FldDescription = mOperation.Tresc1 + mOperation.Tresc2 + mOperation.Tresc3 + mOperation.Tresc4;
        }

        public class XML_WYCIAGI //model nagłówka raportu
        {
            [XmlAttribute("data_od")]
            public String DataOd;
            [XmlAttribute("data_do")]
            public String DataDo;
            [XmlAttribute("rachunek")]
            public String Rachunek;
            [XmlAttribute("opis")]
            public String Opis;
            [XmlAttribute("waluta")]
            public String Waluta;
            [XmlAttribute("nazwa")]
            public String Nazwa;

            [XmlElement("OPERACJA")]
            public List<XML_OPERACJA> Operacje;
        }

        public class XML_OPERACJA //podel pozycji raportu
        {
            [XmlElement("POZYCJA")]
            public String Pozycja;
            [XmlElement("DATA_WALUTY")]
            public String DataWaluty;
            [XmlElement("DATA_KSIEG")]
            public String DataKsiegowania;
            [XmlElement("DATA_PLATNOSCI")]
            public String DataPlatnosci;
            [XmlElement("OPIS")]
            public String Opis;
            [XmlElement("BANK")]
            public String Bank;
            [XmlElement("RACHUNEK")]
            public String Rachunek;
            [XmlElement("KWOTA")]
            public String Kwota;
            [XmlElement("STRONA")]
            public String Strona;
            [XmlElement("WALUTA")]
            public String Waluta;
            [XmlElement("NAZWA1")]
            public String Nazwa1;
            [XmlElement("NAZWA2")]
            public String Nazwa2;
            [XmlElement("NAZWA3")]
            public String Nazwa3;
            [XmlElement("NAZWA4")]
            public String Nazwa4;
            [XmlElement("TRESC1")]
            public String Tresc1;
            [XmlElement("TRESC2")]
            public String Tresc2;
            [XmlElement("TRESC3")]
            public String Tresc3;
            [XmlElement("TRESC4")]
            public String Tresc4;
        }
    }
}
