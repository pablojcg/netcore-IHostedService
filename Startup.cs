using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CapacitacionIHostedService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHostedService, MyBackgroundTask>();
            //services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public class MyBackgroundTask : IHostedService
        {
            public async Task StartAsync(CancellationToken cancellationToken)
            {   
                string path = @"C:\inetpub\wwwmti\logsCapacitacionLog.txt";
                dynamic Data = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")));
                string Time = Data.TimeExec.Time.Value;
                int DelayTime = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    int hour = DateTime.Now.Hour;
                    int minute = DateTime.Now.Minute;
                    string timeExec = hour.ToString() + ":" + minute.ToString();

                    if (DelayTime == 0)
                    {
                        if (timeExec == Time)
                        {
                            string timeDelay = DateTime.Now.AddDays(1).ToShortDateString();
                            DelayTime = CalculateTimeNext(Time, timeDelay);
                            File.AppendAllLines(path, new String[] { "Ya entro por primera Vez DelayTime 0: " + DateTime.Now.ToString() });

                        }
                        else
                        {
                            File.AppendAllLines(path, new String[] { "No se ha ejecutado por primera vez aun DelayTime 0: " + DateTime.Now.ToString() });
                            DelayTime = 5000;
                        }
                    }
                    else {
                        if (timeExec == Time)
                        {
                            string timeDelay = DateTime.Now.AddDays(1).ToShortDateString();
                            DelayTime = CalculateTimeNext(Time, timeDelay);
                            File.AppendAllLines(path, new String[] { "Ya entro por primera Vez DelayTime != 0: " + DateTime.Now.ToString() + " - " + DelayTime.ToString() });
                        }
                        else
                        {
                            File.AppendAllLines(path, new String[] { "No se ha ejecutado por primera vez aun DelayTime != 0: " + DateTime.Now.ToString() });
                            DelayTime = 5000;
                        }
                    }

                    await Task.Delay(DelayTime);
                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public int CalculateTimeNext(string Time, string timeDelay)
            {
                var fecharegistro = DateTime.Parse(timeDelay + " " + Time + ":00");
                var timeSpan = fecharegistro - DateTime.Now;
                int DelayTimeFinal = Convert.ToInt32(timeSpan.TotalMilliseconds);
                return DelayTimeFinal;
            }

        }
    }
}
