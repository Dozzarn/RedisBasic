using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RedisBasic.Models;

namespace RedisBasic.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCache _disturbutedCache;
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
    }
}