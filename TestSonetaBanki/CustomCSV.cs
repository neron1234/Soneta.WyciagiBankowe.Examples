using System;
using System.Collections.Generic;
using System.Text;
using Soneta.Kasa;
using Soneta.WyciagiBankowe.Core;
using TestSonetaBanki;

//rejestracja filtra
[assembly: ImportWyciągówBankowychFilter(
    "Custom (CSV)",
    typeof(CustomCSV))]

namespace TestSonetaBanki
{
    internal class CustomCSV : FilterBaseTXT<CsvAutoMapper>
    {
        public override Version Version => new Version(1, 0);

        protected override Boolean HeaderLess => true; //Raport nie posiada nagłówka

        protected override Char SeparatorMark => ';'; //Ustawienie znaku separatora

        protected override char? QuotationMark => null; //Brak znaku cudzysłowia

        public override string DefaultEXT => "CSV"; //Domyślne rozszerzenie pliku

        public override Encoding GetEncoding() => 
            Encoding.GetEncoding(encodingUTF8); //Ustawienie kodowania pliku

        protected override FeatureSupport Features => //Obsługiwane przez filtr możliwości
            FeatureSupport.SkipItemsWithNonMatchingCurrencies | //Omijanie wpisów z niewłaściwym symbolem waluty
            FeatureSetOperationDate(base.Features, FeatureSupport.OperationDtxNone); //brak daty opcjonalnej

        protected override void extractHeader(ReportHeader reportHeader)
        { } //brak nagłówka

        protected override void PreparePrivateData()
        {
            //ustawienie kolumn wymaganych i opcjonalnych
            PrivateData.Fields.DataKsiegowania.Setup(true, "data");
            PrivateData.Fields.Kwota.Setup(true, "kwota");
            PrivateData.Fields.KwotaSymbol.Setup(true, "waluta");
            PrivateData.Fields.Kontrahent1.Setup(false, "imię i nazwisko");
            PrivateData.Fields.Kontrahent2.Setup(false, "adres", "ulica");
            PrivateData.Fields.Kontrahent3.Setup(false, "kod");
            PrivateData.Fields.Kontrahent4.Setup(false, "miasto");
            PrivateData.Fields.Kontrahent5.Setup(false, "e-mail");
            PrivateData.Fields.Kontrahent6.Setup(false, "telefon");
            PrivateData.Fields.Opis1.Setup(false, "opis");
            PrivateData.Fields.Opis2.Setup(false, "opis2/nr konta");
            PrivateData.Fields.Dodatkowe1.Setup(true, "prowizja");

            //ustawienie kolumn omijanych
            PrivateData.Skip.Add("typ operacji", "trans id", "saldo", "zmiana salda", "order id", "typ płatności", "id sesji", "id zamówienia");

            //Opcje przetwarzania kolumn 
            PrivateData.Rules.DataKsiegowania = Helpers.Dates.FromYMD;
            PrivateData.Rules.Kwota = Helpers.Numeric.ParseDecimal;
            PrivateData.Rules.KwotaSymbol = Helpers.Str.Normalize;
            PrivateData.Rules.Opis = (s1, s2, s3, s4, s5, s6) => Helpers.Str.AppendNL(s1, s2);
            PrivateData.Rules.Kontrahent = (s1, s2, s3, s4, s5, s6) => Helpers.Str.AppendNL(s1, s2, Helpers.Str.AppendSp(s3, s4), s5, s6);

            //wskazanie gdzie znajdują się w pliku nagłówki kolumn
            var header = navigate(NavigationHint.LocationAnyText | NavigationHint.RequiredMandatory);

            //parsowanie nagłówka
            PrivateData.ParseHeader(getParser(header).AllFields); 
        }


        protected override void parseReportItems(IList<ReportItem> reportItems, string opLine)
        { 
            PrivateData.ParseValues(getParser(opLine).AllFields);//parsowanie wiersza

            var reportItem = new ReportItem //tworzenie obiektu raportu
            {
                BookingDate = PrivateData.GetBookingDate(),
                AmountDec = PrivateData.GetAmount(),
                CurrencySymbol = PrivateData.GetAmountSymbol(),
                FldCustomer = PrivateData.GetCustomer(),
                FldDescription = PrivateData.GetDescription(),
            };

            reportItems.Add(reportItem); //dodawanie obiektu raportu do kolekcji 
        }
    }
}
