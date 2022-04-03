using System;
using System.Collections.Generic;
using System.Web.Services;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class CarInfoService : WebService
{    
    [WebMethod] 
    public CarInfo Translate(CarInfo carInfo)
    {
        Translit translit = new Translit();
        carInfo.CarPrice = Math.Round(carInfo.CarPrice / 100, 2);
        carInfo.CarMaxSpeed = Math.Round(carInfo.CarMaxSpeed / 1.609, 3);
        carInfo.CarDescription = translit.Translite(carInfo.CarDescription);
        return carInfo;
    }

}
public class CarInfo
{
    public double CarPrice;
    public string CarDescription;
    public double CarMaxSpeed;
    public CarInfo()
    {
        CarPrice = 0;
        CarDescription = "";
        CarMaxSpeed = 0;    
    }
}
public class Translit
{
    Dictionary<string, string> dictionaryChar = new Dictionary<string, string>()
            {
                {"а","a"},
                {"б","b"},
                {"в","v"},
                {"г","g"},
                {"д","d"},
                {"е","e"},
                {"ё","yo"},
                {"ж","zh"},
                {"з","z"},
                {"и","i"},
                {"й","y"},
                {"к","k"},
                {"л","l"},
                {"м","m"},
                {"н","n"},
                {"о","o"},
                {"п","p"},
                {"р","r"},
                {"с","s"},
                {"т","t"},
                {"у","u"},
                {"ф","f"},
                {"х","h"},
                {"ц","ts"},
                {"ч","ch"},
                {"ш","sh"},
                {"щ","sch"},
                {"ъ","'"},
                {"ы","yi"},
                {"ь",""},
                {"э","e"},
                {"ю","yu"},
                {"я","ya"}
            };
    public string Translite(string source)
    {
        var result = "";
        // проход по строке для поиска символов подлежащих замене которые находятся в словаре dictionaryChar
        foreach (var ch in source)
        {
            var ss = "";
            // берём каждый символ строки и проверяем его на нахождение его в словаре для замены,
            // если в словаре есть ключ с таким значением то получаем true 
            // и добавляем значение из словаря соответствующее ключу
            if (dictionaryChar.TryGetValue(ch.ToString(), out ss))
            {
                result += ss;
            }
            // иначе добавляем тот же символ
            else result += ch;
        }
        return result;
    }
}
    

