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

            Person person = new Person();
            person.Name = "Arya";
            person.Surname = "Kaşmer";
            person.Age = 0;
            person.isMail = false;

            var data = JsonConvert.SerializeObject(person);//serialize
            _disturbutedCache.SetString("Person", data); // keep object in cache
        }
        public async Task<IActionResult> Index()
        {
            var cacheKey = "Time";
            var limit = TimeSpan.FromSeconds(5);
            var existingTime = _disturbutedCache.GetString(cacheKey);//get string with cachekey
            if (string.IsNullOrEmpty(existingTime))
            {
                existingTime = DateTime.UtcNow.ToString();
                var option = new DistributedCacheEntryOptions().SetSlidingExpiration(limit);//when data deduct from cache (timeout)
                option.AbsoluteExpirationRelativeToNow = limit;// latest deduct time(latest timeout
                await _disturbutedCache.SetStringAsync(cacheKey, $"{existingTime}", option);// change cachekey string with existingtime, after that set timeout we selected before
            }
            ViewBag.Time = await _disturbutedCache.GetStringAsync(cacheKey);//get string from cache
            var person = await _disturbutedCache.GetStringAsync("Person");//get person object data from cache
            var person2 = JsonConvert.DeserializeObject<Person>(person);// deserialize 
            return View(person2);
        }
    }
}