using Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static WPF_RESNET.MainWindow;

namespace WPF_RESNET
{
    public class Client
    {
        HttpClient httpClient = new HttpClient();
        public event Action OnProcessedImage;
        public event Action OnConnectionFailed;
        public event Action OnGetStatistic;
        ConcurrentQueue<SendData> queue = new ConcurrentQueue<SendData>();
        ConcurrentQueue<SendData> statQueue = new ConcurrentQueue<SendData>();
        public SendData GetResult()
        {
            if (queue.TryDequeue(out var result))
                return result;
            else
                return null;
        }
        public async void ProcessPicture(string file)
        {
            var data = new SendData();
            data.Path = file;
            data.Data = Convert.ToBase64String(System.IO.File.ReadAllBytes(file));

            var j = JsonConvert.SerializeObject(data);
            var c = new StringContent(j);
            c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            try { 
                var answer = await httpClient.PutAsync("http://localhost:5000/main", c);
                data.TypeName = await answer.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(data.TypeName))
                    return;
                queue.Enqueue(data);
                OnProcessedImage?.Invoke();
            }
            catch
            {
                OnConnectionFailed?.Invoke();
            }
        }
        public async void Clear()
        {
            try
            {
                await httpClient.DeleteAsync("http://localhost:5000/main");
            }
            catch
            {
                OnConnectionFailed?.Invoke();
            }
        }
        public async void GetAll()
        {
            try
            {

                var result = await httpClient.GetStringAsync("http://localhost:5000/main");
                foreach (var i in JsonConvert.DeserializeObject<SendData[]>(result))
                {
                    queue.Enqueue(i);
                    OnProcessedImage();
                }
            }
            catch
            {
                OnConnectionFailed();
            }
        }
        public SendData GetStatistic()
        {
            if (statQueue.TryDequeue(out var result))
                return result;
            else
                return null;
        }
        public async void GetStatistic(SendDataView input)
        {
            try
            {
                var data = new SendData();
                data.Data = input.Data.GetDeterministicHashCode().ToString();
                data.TypeName = input.TypeName;
                var j = JsonConvert.SerializeObject(data);
                var c = new StringContent(j);
                c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var result = await httpClient.PutAsync("http://localhost:5000/main/", c);
                
                data.TypeName = await result.Content.ReadAsStringAsync();
                statQueue.Enqueue(data);
                OnGetStatistic?.Invoke();
            }
            catch
            {
                OnConnectionFailed?.Invoke();
            }
        }
    }
}
