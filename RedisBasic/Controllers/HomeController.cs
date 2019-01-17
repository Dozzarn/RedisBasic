using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RedisBasic.Models;
using StackExchange.Redis;

namespace RedisBasic.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCache _disturbutedCache;
        private ConnectionMultiplexer Connection;
        public HomeController(IDistributedCache distributedCache)
        {
          
            _disturbutedCache = distributedCache;
        }
        

        public async Task<IActionResult> Index()
        {
           Person person = new Person
            {
                Name = "Arya",
                Surname = "Kaşmer",
                Age = 0,
                isMail = false
            };
            var cacheKey = "Time";
            var slide = TimeSpan.FromSeconds(5);
            var max = TimeSpan.FromSeconds(5);
            var existingTime = _disturbutedCache.GetString(cacheKey);//get string with cachekey
            if (string.IsNullOrEmpty(existingTime))
            {
                existingTime = DateTime.UtcNow.ToString();
                var option = new DistributedCacheEntryOptions();
                //option.SetSlidingExpiration(slide); after first work it sliding Expiration  5 sec
                option.AbsoluteExpirationRelativeToNow = max;//Expiration Time
                var data = JsonConvert.SerializeObject(person);//serialize
                _disturbutedCache.SetString("Person", data, option); // keep object in cache
                await _disturbutedCache.SetStringAsync(cacheKey, $"{existingTime}", option);// change cachekey string with existingtime, after that set timeout we selected before
            }
            ViewBag.Time = await _disturbutedCache.GetStringAsync(cacheKey);//get string from cache
            var person1 = await _disturbutedCache.GetStringAsync("Person");//get person object data from cache
            var person2 = JsonConvert.DeserializeObject<Person>(person1);// deserialize 
            return View(person2);
        }

        [Route("stack")]
        public async Task<IActionResult> Stack()
        {
            Person person = new Person
            {
                Name = "Arya",
                Surname = "Kaşmer",
                Age = 0,
                isMail = false
            };

            Connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
            IDatabaseAsync db = Connection.GetDatabase(1);
            var exp = TimeSpan.FromSeconds(5);


            var cacheData = await db.StringGetAsync("Data1");
            if (string.IsNullOrEmpty(cacheData))
            {
                var data = JsonConvert.SerializeObject(person);
                await db.StringSetAsync("Data1", data, exp);

            }
            var person1 = await db.StringGetAsync("Data1");
            var person2 = JsonConvert.DeserializeObject<Person>(person1);
            return View(person2);
        }
    }
}