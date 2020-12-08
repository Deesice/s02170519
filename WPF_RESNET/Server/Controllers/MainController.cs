using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        ResNet resNet = new ResNet();
        LibraryContext libraryContext;
        public MainController(LibraryContext l)
        {
            libraryContext = l;
        }
        //[HttpGet]
        //public SendData[] Get()
        //{
        //    return libraryContext.GetAll().ToArray();
        //}
        [HttpGet]
        public SendData[] Get()
        {
                var all = libraryContext.GetAll().ToArray();
                List<SendData> list = new List<SendData>();
                for (int i = 0; i < (all.Count() - 1) % 10 + 1; i++)
                    list.Add(null);
                return list.ToArray();
        }
        [HttpGet ("{page}")]
        public SendData[] Get(int page)
        {
            var all = libraryContext.GetAll().ToArray();
            List<SendData> list = new List<SendData>();
            for (int i = page * 10; i < Math.Min((page + 1) * 10, all.Count()); i++)
                list.Add(all[i]);
            return list.ToArray();
        }
        [HttpPut]
        public string Put(SendData input)
        {
            if (!string.IsNullOrEmpty(input.TypeName))
            {
                int i = 0;
                Console.WriteLine(input.Data);
                lock (libraryContext)
                    i = libraryContext.GetStatistic(int.Parse(input.Data));
                return i.ToString();
            }
            Console.WriteLine(input.Data.GetDeterministicHashCode());
            lock (libraryContext)
                input.TypeName = libraryContext.GetFile(input);
            if (string.IsNullOrEmpty(input.TypeName))
            {
                try
                {
                    input.TypeName = resNet.ProcessImage(input.Data);
                    lock (libraryContext)
                        libraryContext.AddResNetResult(input);
                }
                catch
                {
                    input.TypeName = "";
                }
            }
            return input.TypeName;
        }
        [HttpDelete]
        public void Delete()
        {
            lock(libraryContext)
                libraryContext.Clear();
        }
    }
}
