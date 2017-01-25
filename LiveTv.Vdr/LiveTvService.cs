using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.LiveTv;
using LiveTv.Vdr.RestfulApi.Client;
using LiveTv.Vdr.RestfulApi.Resources;
using LiveTv.Vdr.RestfulApi;

namespace LiveTv.Vdr
{
    class LiveTvService : ILiveTvService
    {
        #region Fields

        private readonly IRestfulApiClient _restfulApiClient;
        //private readonly IHttpClient _httpClient;
        //private readonly IJsonSerializer _jsonSerializer;
        public readonly ILogger _logger;

        #endregion 

        #region Properties

        public string HomePageUrl
        {
            get { return "http://www.tvdr.de/"; }
        }

        public string Name
        {
            get { return "LiveTV.Vdr"; }
        }       

        private IRestfulApiClient RestfulApiClient
        {
            get { return _restfulApiClient; }
        }

        #endregion

        #region Events

        public event EventHandler DataSourceChanged;
        public event EventHandler<RecordingStatusChangedEventArgs> RecordingStatusChanged;

        #endregion

        #region Ctor

        public LiveTvService(IHttpClient httpClient, IJsonSerializer jsonSerializer, ILogger logger)
        {
            _logger = logger;
            _restfulApiClient = new VdrRestfulApiClient(httpClient, jsonSerializer);

            _logger.Info("[LiveTV.Vdr] Service created.");
        }

        #endregion

        public Task CancelSeriesTimerAsync(string timerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CancelTimerAsync(string timerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CloseLiveStream(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CreateSeriesTimerAsync(SeriesTimerInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CreateTimerAsync(TimerInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRecordingAsync(string recordingId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ImageStream> GetChannelImageAsync(string channelId, CancellationToken cancellationToken)
        {
            // Leave as is. This is handled by supplying image url to ChannelInfo
	    throw new NotImplementedException();
        }

        public async Task<IEnumerable<ChannelInfo>> GetChannelsAsync(CancellationToken cancellationToken)
        {
            _logger.Info("[LiveTV.Vdr]  {0}...", nameof(GetChannelsAsync));

            var channelsResource = await RestfulApiClient.RequestChannelsResource(cancellationToken);
            
            var channelInfoList = new List<ChannelInfo>();
            foreach (ChannelResource chRes in channelsResource.Channels)
            {
                channelInfoList.Add(Converters.ChannelResourceToChannelInfo(chRes));                    
            }

            return channelInfoList;
        }

        public async Task<MediaSourceInfo> GetChannelStream(string channelId, string streamId, CancellationToken cancellationToken)
        {
            _logger.Info("[LiveTV.Vdr] Start ChannelStream");
            var config = Plugin.Instance.Configuration;
            var baseUrl = "http://" + Plugin.Instance.Configuration.VDR_ServerName + ":" + Plugin.Instance.Configuration.VDR_HTTP_Port;

            string streamUrl = string.Format("{0}/TS/{1}", baseUrl, channelId);
            _logger.Info("[LiveTV.Vdr] Streaming " + streamUrl);
            return new MediaSourceInfo
            {
                Id = channelId,
                Path = streamUrl,
                Protocol = MediaProtocol.Http,
                MediaStreams = new List<MediaStream>
                        {
                            new MediaStream
                            {
                                Type = MediaStreamType.Video,
                                IsInterlaced = true,
                                // Set the index to -1 because we don't know the exact index of the video stream within the container
                                Index = -1,
                            },
                            new MediaStream
                            {
                                Type = MediaStreamType.Audio,
                                IsInterlaced = true,
                                // Set the index to -1 because we don't know the exact index of the audio stream within the container
                                Index = -1
                            }
                        },
                // This takes too long
                SupportsProbing = false
            };

        }

        public Task<List<MediaSourceInfo>> GetChannelStreamMediaSources(string channelId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<SeriesTimerInfo> GetNewTimerDefaultsAsync(CancellationToken cancellationToken, ProgramInfo program = null)
        {
            throw new NotImplementedException();
        }

        public Task<ImageStream> GetProgramImageAsync(string programId, string channelId, CancellationToken cancellationToken)
        {
            // Leave as is. This is handled by supplying image url to ChannelInfo
            throw new NotImplementedException();
            //return null;
        }

        public async Task<IEnumerable<ProgramInfo>> GetProgramsAsync(string channelId, DateTime startDateUtc, DateTime endDateUtc, CancellationToken cancellationToken)
        {
            _logger.Info("[LiveTV.Vdr]  {0}: ChannelID={1}, StartDate={2}, EndDate={3}", nameof(GetProgramsAsync), channelId, startDateUtc, endDateUtc);
            List<ProgramInfo> programInfoList = new List<ProgramInfo>();

            var eventsResource = await RestfulApiClient.RequestEventsResource(cancellationToken, channelId, endDateUtc);

            foreach (EventResource eventRes in eventsResource.Events)
            {
                programInfoList.Add(Converters.EventResourceToProgramInfo(eventRes));
            }
            
            return programInfoList;

            /*
              _logger.Info("[NextPvr] Start GetPrograms Async, retrieve all Programs");
            await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);
            var baseUrl = Plugin.Instance.Configuration.WebServiceUrl;

            var options = new HttpRequestOptions()
            {
                CancellationToken = cancellationToken,
                Url = string.Format("{0}/public/GuideService/Listing?sid={1}&stime={2}&etime={3}&channelId={4}",
                baseUrl, Sid,
                ApiHelper.GetCurrentUnixTimestampSeconds(startDateUtc).ToString(_usCulture),
                ApiHelper.GetCurrentUnixTimestampSeconds(endDateUtc).ToString(_usCulture),
                channelId)
            };

            using (var stream = await _httpClient.Get(options).ConfigureAwait(false))
            {
                return new ListingsResponse(baseUrl).GetPrograms(stream, _jsonSerializer, channelId, _logger).ToList();
            } 
             */
        }

        public Task<ImageStream> GetRecordingImageAsync(string recordingId, CancellationToken cancellationToken)
        {
            _logger.Info("[LiveTV.Vdr]  {0} not implemented", nameof(GetRecordingImageAsync));
	    throw new NotImplementedException();
        }

        public async Task<IEnumerable<RecordingInfo>> GetRecordingsAsync(CancellationToken cancellationToken)
        {
            _logger.Info("[LiveTV.Vdr]  {0}..", nameof(GetRecordingsAsync));
            List<RecordingInfo> recordingInfoList = new List<RecordingInfo>();

            var recordingsResource = await RestfulApiClient.RequestRecordingsResource(cancellationToken);

            foreach (RecordingResource recRes in recordingsResource.Recordings)
            {
                recordingInfoList.Add(Converters.RecordingResourceToRecordingInfo(recRes));
            }

            return recordingInfoList;
        }

        public async Task<MediaSourceInfo> GetRecordingStream(string recordingId, string streamId, CancellationToken cancellationToken)
        {
                   _logger.Info("[LiveTV.Vdr] Start RecordingStream");
            var config = Plugin.Instance.Configuration;
            var baseUrl = "http://" + Plugin.Instance.Configuration.VDR_ServerName + ":" + Plugin.Instance.Configuration.VDR_HTTP_Port;

            string streamUrl = string.Format("{0}/{1}.rec.ts", baseUrl, int.Parse(recordingId) + 1);
            _logger.Info("[LiveTV.Vdr] Streaming " + streamUrl);
            return new MediaSourceInfo
            {
                Id = recordingId,
                Path = streamUrl,
                Protocol = MediaProtocol.Http,
                MediaStreams = new List<MediaStream>
                        {
                            new MediaStream
                            {
                                Type = MediaStreamType.Video,
                                IsInterlaced = true,
                                // Set the index to -1 because we don't know the exact index of the video stream within the container
                                Index = -1,
                            },
                            new MediaStream
                            {
                                Type = MediaStreamType.Audio,
                                IsInterlaced = true,
                                // Set the index to -1 because we don't know the exact index of the audio stream within the container
                                Index = -1
                            }
                        },
                // This takes too long
                SupportsProbing = false
            };


        }

        public Task<List<MediaSourceInfo>> GetRecordingStreamMediaSources(string recordingId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SeriesTimerInfo>> GetSeriesTimersAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        //TODO: think about ConnectionCheckAsync (sse nextpvr)
        public async Task<LiveTvServiceStatusInfo> GetStatusInfoAsync(CancellationToken cancellationToken)
        {            
            _logger.Info("[LiveTV.Vdr]  {0}...", nameof(GetStatusInfoAsync));

            var infoResource = await RestfulApiClient.RequestInfoResource(cancellationToken);

            if (infoResource == null)
                return new LiveTvServiceStatusInfo { Status = LiveTvServiceStatus.Unavailable };

            return new LiveTvServiceStatusInfo
            {
                Status = LiveTvServiceStatus.Ok,
                Version = infoResource.Vdr.Version,
                Tuners = new List<LiveTvTunerInfo>() //TODO:
            };
        }

        public async Task<IEnumerable<TimerInfo>> GetTimersAsync(CancellationToken cancellationToken)
        {
            _logger.Info("[LiveTV.Vdr]  {0}...", nameof(GetTimersAsync));
            List<TimerInfo> timerInfoList = new List<TimerInfo>();

            var timersResource = await RestfulApiClient.RequestTimersResource(cancellationToken);
            foreach (Timer timerRes in timersResource.Timers)
            {
                timerInfoList.Add(Converters.TimerResourceToTimerInfo(timerRes));
            }

            return timerInfoList;
        }

        public Task RecordLiveStream(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ResetTuner(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSeriesTimerAsync(SeriesTimerInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTimerAsync(TimerInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
