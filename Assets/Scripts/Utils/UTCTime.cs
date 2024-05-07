using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Utils
{
    public class UTCTime : MonoBehaviour
    {
        private const string _url = "http://worldtimeapi.org/api/ip";
        private const string _timeFormat = @"\d{2}:\d{2}:\d{2}";
        private const string _dateFormat = @"\d{4}-\d{2}-\d{2}";

        public bool IsTimeLodaed { get; private set; } = false;
        public DateTime CurrentDateTime { get; private set; }

        public void GetRealDateTimeFromURL()
        {
            IsTimeLodaed = false;
            StartCoroutine(Get());
        }

        private IEnumerator Get()
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(_url);

            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
                Debug.Log("Error: " + webRequest.error);
            else
            {
                CurrentDateTime = ParseDateTime(DatetimeStringFromURL(webRequest));
                IsTimeLodaed = true;
            }
        }

        private static string DatetimeStringFromURL(UnityWebRequest webRequest) =>
            Regex.Match(webRequest.downloadHandler.text, @"\w*utc_datetime\W{3}" + _dateFormat + @"T" + _timeFormat)
                .Value;

        DateTime ParseDateTime(string datetime) =>
            DateTime.Parse($"{Date(datetime)} {Time(datetime)}");

        private static string Time(string datetime) =>
            Regex.Match(datetime, _dateFormat).Value;

        private static string Date(string datetime) =>
            Regex.Match(datetime, _timeFormat).Value;
    }
}