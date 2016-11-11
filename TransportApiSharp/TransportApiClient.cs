﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TransportAPISharp
{
    /// <summary>
    /// A .Net client for the <a href="http://www.transportapi.com">TransportAPI</a> UK public
    /// transport API.
    /// </summary>
    public class TransportApiClient : IDisposable
    {
        private const string BaseUrl = "http://transportapi.com/v3/";
        private readonly string _appId;
        private readonly string _appKey;

        private HttpClient _httpClient;

        /// <summary>
        /// Gets the last error message generated by a method.
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Initialises a new <c>TransportApiClient</c> class.
        /// </summary>
        /// <param name="appId">The application's Id. Get this from the TransportAPI site.</param>
        /// <param name="appKey">The application's key. Get this from the TransportAPI site.</param>
        /// <param name="handler">
        /// Used to inject an alternative <c>System.Net.Http.HttpMessageHandler</c>. Used for unit testing. Defaults to null.
        /// </param>
        public TransportApiClient(string appId, string appKey, HttpMessageHandler handler = null)
        {
            _appId = appId;
            _appKey = appKey;
            if (handler != null)
                _httpClient = new HttpClient(handler);
            else
                _httpClient = new HttpClient();
        }

        /// <summary>
        /// Returns bus stops near a given geographic position.
        /// </summary>
        /// <param name="lat">The latitude of the position in question.</param>
        /// <param name="lon">The longitude of the position in question.</param>
        /// <param name="page">The page number of the result set. Defaults to 1.</param>
        /// <param name="stopsPerPage">The number of stops per page. Defaults to 25.</param>
        /// <returns>A <c>BusStopsNearResponse</c> class, or null if there was an error.</returns>
        /// <remarks>If there is an error, <c>LastError</c> will contain the error message.</remarks>
        public async Task<BusStopsNearResponse> BusStopsNear(double lat, double lon, int page = 1, int stopsPerPage = 25)
        {
            var task = await _httpClient.GetAsync("http://transportapi.com/v3/uk/bus/stops/near.json?"
                + $"app_id={_appId}&app_key={_appKey}"
                + $"&lat={lat}&lon={lon}&page={page}&rpp={stopsPerPage}");

            var jsonString = await task.Content.ReadAsStringAsync();
            return deserializeResponse<BusStopsNearResponse>(jsonString);
        }

        /// <summary>
        /// Returns bus stops in a bounding box (a geographical rectangle).
        /// </summary>
        /// <param name="north">The northernmost latitude in the rectangle.</param>
        /// <param name="south">The southernmost latitude in the rectangle.</param>
        /// <param name="east">The easternmost longitude in the bounding box.</param>
        /// <param name="west">The westernmost longitude in the bounding box.</param>
        /// <param name="page">The page number of the results set.</param>
        /// <param name="stopsPerPage">The stops per page.</param>
        /// <returns>A <c>BusStopsNearResponse</c> class, or null if there was an error.</returns>
        /// <remarks>If there is an error, <c>LastError</c> will contain the error message.</remarks>
        public async Task<BusStopsNearResponse> BusStopsInBoundingBox(double north, double south, double east, double west, int page = 1, int stopsPerPage = 25)
        {
            var task = await _httpClient.GetAsync(BaseUrl +
                "uk/bus/stops/bbox.json?"
                + $"app_id={_appId}&app_key={_appKey}"
                + $"&maxlat={north}&maxlon={east}"
                + $"&minlat={south}&minlon={west}"
                + $"&page={page}&rpp={stopsPerPage}");

            var jsonString = await task.Content.ReadAsStringAsync();
            return deserializeResponse<BusStopsNearResponse>(jsonString);
        }

        /// <summary>
        /// Returns buses departing from the given bus stop in the coming hour.
        /// </summary>
        /// <param name="atcoCode">The <a href="http://docs.transportapi.com/index.html?raml=http://transportapi.com/v3/raml/transportapi.raml##bus_information">ATCO code</a> of the bus stop of interest.</param>
        /// <param name="dateTime">The date & time of interest.</param>
        /// <param name="group">If set to <c>true</c>, groups the departures by bus route, otherwise returns just one group.</param>
        /// <param name="limit">The number of departures to return in each group. Defaults to 3.</param>
        /// <returns>A <c>BusTimetableResponse</c> class, or null if there was an error.</returns>
        /// <remarks>If there is an error, <c>LastError</c> will contain the error message.</remarks>
        public async Task<BusTimetableResponse> BusTimetable(string atcoCode, DateTime dateTime, bool group = true, int limit = 3)
        {
            var date = dateTime.ToString("yyyy-MM-dd");
            var time = dateTime.ToString("HH:mm");

            var groupValue = (group) ? "route" : "no";

            var task = await _httpClient.GetAsync(BaseUrl +
                $"uk/bus/stop/{atcoCode}/{date}/{time}/timetable.json?"
                + $"app_id={_appId}&app_key={_appKey}"
                + $"&group={groupValue}&limit={limit}");

            var jsonString = await task.Content.ReadAsStringAsync();

            return deserializeResponse<BusTimetableResponse>(jsonString);
        }

        /// <summary>
        /// Returns bus departures from a given stop.
        /// </summary>
        /// <param name="atcoCode">The <a href="http://docs.transportapi.com/index.html?raml=http://transportapi.com/v3/raml/transportapi.raml##bus_information">ATCO code</a> of the bus stop of interest.</param>
        /// <param name="group">If set to <c>true</c>, groups the departures by bus route, otherwise returns just one group.</param>
        /// <param name="limit">The number of departures to return in each group. Defaults to 3.</param>
        /// <param name="nextBuses">If set to <c>true</c> the function uses the NextBuses datasource. Defaults to <c>false</c>.</param>
        /// <returns>A <c>BusTimetableResponse</c> class, or null if there was an error.</returns>
        /// <remarks>If there is an error, <c>LastError</c> will contain the error message.
        /// Live bus data is not available in all areas, and in some areas outside of London and a few others is only available from the NextBuses datasource. This costs 10 hits.
        /// </remarks>
        public async Task<BusLiveResponse> BusLive(string atcoCode, bool group = true, int limit = 3, bool nextBuses = false)
        {
            var groupValue = (group) ? "route" : "no";
            var nextBusesValue = (nextBuses) ? "yes" : "no";

            var task = await _httpClient.GetAsync(BaseUrl +
                $"uk/bus/stop/{atcoCode}/live.json?"
                + $"app_id={_appId}&app_key={_appKey}"
                + $"&group={groupValue}&limit={limit}&nextbuses={nextBusesValue}");

            var jsonString = await task.Content.ReadAsStringAsync();

            return deserializeResponse<BusLiveResponse>(jsonString);
        }

        /// <summary>
        /// Returns the route of a specific bus from a specific stop at a specific <c>DateTime</c>.
        /// </summary>
        /// <param name="atcoCode">The <a href="http://docs.transportapi.com/index.html?raml=http://transportapi.com/v3/raml/transportapi.raml##bus_information">ATCO code</a> of the bus stop of interest.</param>
        /// <param name="direction">The direction of the bus. Usually 'inbound' (towards a city centre) or 'outbound' (away from a city centre), but other values such as 'clockwise' may be valid too.</param>
        /// <param name="lineName">The bus route or line of interest.</param>
        /// <param name="operatorCode">The <a href="http://docs.transportapi.com/index.html?raml=http://transportapi.com/v3/raml/transportapi.raml##bus_information">operator code</a> of the bus operator.</param>
        /// <param name="dateTime">The date & time of interest.</param>
        /// <returns>A <c>BusRouteTimetableResponse</c> class, or null if there was an error.</returns>
        /// <remarks>If there is an error, <c>LastError</c> will contain the error message.</remarks>
        public async Task<BusRouteTimetableResponse> BusRouteTimetable(string atcoCode, string direction, string lineName, string operatorCode, DateTime dateTime)
        {
            var date = dateTime.ToString("yyyy-mm-dd");
            var time = dateTime.ToString("HH:mm");

            var task = await _httpClient.GetAsync(BaseUrl +
                $"uk/bus/route/{operatorCode}/{lineName}/{direction}/{atcoCode}/{date}/{time}/timetable.json?"
                + $"app_id={_appId}&app_key={_appKey}");

            var jsonString = await task.Content.ReadAsStringAsync();

            return deserializeResponse<BusRouteTimetableResponse>(jsonString);
        }

        /// <summary>
        /// Gets a list of bus operators.
        /// </summary>
        /// <returns>A <c>List</c> of <c>BusOperator</c> classes.</returns>
        public async Task<List<BusOperator>> GetBusOperators()
        {
            var task = await _httpClient.GetAsync(BaseUrl +
                $"uk/bus/operators.json?"
                + $"app_id={_appId}&app_key={_appKey}");

            var jsonString = await task.Content.ReadAsStringAsync();

            return deserializeResponse<List<BusOperator>>(jsonString);
        }

        private T deserializeResponse<T>(string responseString) where T : class
        {
            T returnVal = null;

            // Valid responses are a JSON array, a JSON object with valid data or a JSON object with
            // an 'error' key. Parse the response so we can see if it's an array or an object...
            var parsedResponse = JToken.Parse(responseString);

            if (parsedResponse is JArray)
                // Response is a JSON array, so it's not an error.
                returnVal = JsonConvert.DeserializeObject<T>(responseString);
            else
            {
                // Response is a JSON object. Check to see if it contains an 'error' key...
                var checkForErrors = parsedResponse["error"];
                if (checkForErrors == null)
                    returnVal = JsonConvert.DeserializeObject<T>(responseString);
                else
                    LastError = checkForErrors.Value<string>();
            }

            return returnVal;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}