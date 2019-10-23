using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WhatIsConcurrency
{
    public class Concurrency
    {

        public async Task ThrowNotImplementedException()
        {
            throw new NotImplementedException();
        }

        public async Task ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }

        public async Task ObserveOneException()
        {
            var t1 = ThrowNotImplementedException();
            var t2 = ThrowInvalidOperationException();

            try
            {
                await Task.WhenAll(t1, t2);
            }
            catch (Exception)
            {
                //这里只会抛出一个错误，要么t1,要么t2
                throw;
            }
        }

        public async Task ObserveAllException()
        {
            var t1 = ThrowNotImplementedException();
            var t2 = ThrowInvalidOperationException();

            Task allTasks = Task.WhenAll(t1, t2);

            try
            {
                await allTasks;
            }
            catch (Exception ee)
            {
                AggregateException ae = allTasks.Exception;

                throw;
            }
        }

        private async Task<int> GetONCompleteTask(string url1,string url2)
        {
            using (HttpClient client = new HttpClient())
            {
                Task<Byte[]> t1 = client.GetByteArrayAsync(url1);
                Task<Byte[]> t2 = client.GetByteArrayAsync(url2);

                Task<Byte[]> completedTask = 
                    await Task.WhenAny(t1, t2);

                byte[] b1 = await completedTask;

                return b1.Length;
            }
        }

        //并发且顺序执行
        private async Task<int> DelayAndReturn(int val)
        {
            await Task.Delay(TimeSpan.FromSeconds(val));
            return val;
        }

        //并发且顺序执行
        private async Task Approach(Task<int> task)
        {
            var result = await task;
            Trace.WriteLine(result);
        }

        private async Task ProcessTaskAsync()
        {
            Task<int> t2 = DelayAndReturn(2);
            Task<int> t1 = DelayAndReturn(1);
            Task<int> t3 = DelayAndReturn(3);
            Task<int> t4 = DelayAndReturn(4);

            var tasks = 
                new[] { t1, t2, t3, t4 };

            var processTask = (from t in tasks
                               select Approach(t)).ToArray();

            await Task.WhenAll(processTask);
        }

        private async Task ProcessTaskAsyncNext()
        {
            Task<int> t2 = DelayAndReturn(2);
            Task<int> t1 = DelayAndReturn(1);
            Task<int> t3 = DelayAndReturn(3);
            Task<int> t4 = DelayAndReturn(4);

            var tasks =
                new[] { t1, t2, t3, t4 };

            var processTask = tasks.Select(async t =>
           {
               var result = await t;
               Trace.WriteLine(result);
           });

            await Task.WhenAll(processTask);
        }

        //阻止async任务被await调用后恢复运行时，会在原来的上下文中运行
        private async Task ResumeWithoutContext()
        {
            await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
        }

    }
}
