using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;
using ServiceStack.Logging.Log4Net;
using ServiceStack.Redis;


namespace RedisTesting
{
    class Program
    {
        static void Main(string[] args)
        {

            LogManager.LogFactory = new Log4NetFactory("log4net.config");

            // ********************
            // set REDIS CONFIGS
            // ********************
            RedisConfig.DefaultConnectTimeout = 1 * 1000;
            RedisConfig.DefaultSendTimeout = 1 * 1000;
            RedisConfig.DefaultReceiveTimeout = 1 * 1000;
            RedisConfig.DefaultRetryTimeout = 15 * 1000;
            RedisConfig.DefaultIdleTimeOutSecs = 240;
            RedisConfig.BackOffMultiplier = 10;
            RedisConfig.BufferLength = 1450;
            RedisConfig.BufferPoolMaxSize = 500000;
            RedisConfig.VerifyMasterConnections = true;
            RedisConfig.HostLookupTimeoutMs = 1000;
            RedisConfig.DeactivatedClientsExpiry = TimeSpan.FromSeconds(15);
            RedisConfig.DisableVerboseLogging = false;
            
            var redisManager = new RedisManagerPool("localhost:56565?connectTimeout=1000");

            // how many test items to create
            var items = 5;
            // how long to try popping
            var waitForSeconds = 30;
            // name of list
            var listID = "testlist";

            var startedAt = DateTime.Now;

            LogManager.LogFactory.GetLogger("redistest").Info("--------------------------");
            LogManager.LogFactory.GetLogger("redistest").Info("push {0} items to a list, then try pop for {1} seconds. repeat.".Fmt(items, waitForSeconds));
            LogManager.LogFactory.GetLogger("redistest").Info("--------------------------");

            using (var redis = redisManager.GetClient())
            {
                do
                {
                    // add items to list
                    for (int i = 1; i <= items; i++)
                    {
                        redis.PushItemToList(listID, "item {0}".Fmt(i));
                    }

                    do
                    {
                        var item = redis.BlockingPopItemFromList(listID, null);

                        // log the popped item.  if BRPOP timeout is null and list empty, I do not expect to print anything
                        LogManager.LogFactory.GetLogger("redistest").InfoFormat("{0}", item.IsNullOrEmpty() ? " list empty " : item);

                        System.Threading.Thread.Sleep(1000);

                    } while (DateTime.Now - startedAt < TimeSpan.FromSeconds(waitForSeconds));

                    LogManager.LogFactory.GetLogger("redistest").Info("--------------------------");
                    LogManager.LogFactory.GetLogger("redistest").Info("completed first loop");
                    LogManager.LogFactory.GetLogger("redistest").Info("--------------------------");

                } while (DateTime.Now - startedAt < TimeSpan.FromSeconds(2*waitForSeconds));

                LogManager.LogFactory.GetLogger("redistest").Info("--------------------------");
                LogManager.LogFactory.GetLogger("redistest").Info("completed outer loop");
            }
        }
    }
}
