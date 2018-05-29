using System;
using System.Text;
using Soneta.Kasa;
using Soneta.WyciagiBankowe.Core;
using Soneta.WyciagiBankowe.Filters.Standard.MT940;

//rejestracja filtra
[assembly: ImportWyciągówBankowychFilter(
    "Custom (MT940)",
    typeof(CustomMT940))]

namespace Soneta.WyciagiBankowe.Filters.Standard.MT940
{
    internal sealed class CustomMT940 : FilterBaseMT940
    {
        public override String DefaultEXT => "Mt940"; //domyślne rozszerzenie pliku

        public override Version Version => new Version(1, 0); //werja filtra

        public override Encoding GetEncoding() => Encoding.GetEncoding(encodingEurDos); //kodowanie pliku
               
        protected override void ConfigureBody86Parser(Body86.Body86Extension body86Extension) //konfiguracja parsera pola 86
        {
            body86Extension.AllowLettersInOperationID = true; //Dopuszczamy litery w identyfikatorach
            body86Extension.OperationIdValidator = OpCodeValidator; //Walidator identyfikatorów
            body86Extension.DupKeyPolicy = Body86.DuplicateKeyPolicy.Concatenate; //Tryb reakcji na zduplikowane klucze w polu - dopisywanie
        }
        
        private bool OpCodeValidator(String opCode)
        {
            if (IsValidOpCode(opCode)) //wykorzystanie standardowego walidatora
                return true;
            //dodatkowa walidacja niestandardowych kodów
            return opCode.Length == 3 && Char.IsLetter(opCode[0]) && Char.IsDigit(opCode[1]) && Char.IsDigit(opCode[2]);
        }


        protected override void ParseReportItem(ReportItem reportItem, Body86 body)
        {
            body.Skip(0, 34); //pomijanie identyfikatorów 0 i 34

            reportItem.FldAccount = body.NumerRachunku_38_3031(); //budowanie numeru rachunku w oparciu o pola 38, 30 i 31
            reportItem.FldCustomer = body.Get(32, 33, 60, 61, 62, 63); // budowanie nazwy kontrahenta w oparciu o wskazane pola

            var opisStandard = body.Get(20, 21, 22, 23, 24, 25); //budowanie opisu w oparciu o wskazane pola
            if (opisStandard == String.Empty)
                opisStandard = body.Get(0);

            var app = StrAppender.Instance(opisStandard); //tworzenie opisu przy użyciu klasy StrAppender
            app.SetSeparator(Environment.NewLine + "--" + Environment.NewLine);
            body.GetOthers(app);

            reportItem.FldDescription = app.ToString(); //zapisanie opisu
        }
    }
}
