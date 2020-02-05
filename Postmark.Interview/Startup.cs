using Akka.Actor;
using Akka.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Postmark.BussinessLogic;
using Postmark.BussinessLogic.Interfaces;
using Postmark.WebAPI;
using PostMark.Akka.Actors;
using PostMark.PersistentStorage;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace Postmark.Interview
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
            services.AddControllers();
            
            SetUpActorSystem(services);

            SetupPersistentStorageActor(services);

            SetupEmailSenderActor(services);

            SetupBussinessLogicActor(services);

            SetupApiListenerActor(services);
        }

        private static void SetUpActorSystem(IServiceCollection services)
        {
            services.AddSingleton(_ =>
                        (
                            ActorSystem.Create
                            (
                                "PostMarkWebAPI",
                                ConfigurationLoader.Load()
                            )
                        ));
        }

        private static void SetupApiListenerActor(IServiceCollection services)
        {
            services.AddSingleton<ApiListenerActorProvider>(provider =>
            {
                var actorSystem = provider.GetService<ActorSystem>();

                var bussinessLogicActor = provider.GetService<BussinessLogicActorProvider>()();

                var actor = actorSystem.ActorOf
                (
                    ApiListenerActor.Create(BussinessLogicActor: bussinessLogicActor)
                    .WithRouter(new RoundRobinPool(2)),
                    nameof(ApiListenerActor)
                );

                return () => actor;
            });
        }

        private static void SetupPersistentStorageActor(IServiceCollection services)
        {
            services.AddSingleton<PersistentStorageActorProvider>(provider =>
            {
                var actorSystem = provider.GetService<ActorSystem>();

                var actor = actorSystem.ActorOf
                (
                    PersistentStorageActor.Create
                    (
                       new RedisStorage()
                    ).WithRouter(new RoundRobinPool(2)),
                    nameof(PersistentStorageActor)
                ); ;
                return () => actor;
            });
        }

        private static void SetupEmailSenderActor(IServiceCollection services)
        {
            services.AddSingleton<EmailSenderActorProvider>(provider =>
            {
                var actorSystem = provider.GetService<ActorSystem>();

                var smtpClient = new EmailNotiificationActor.SmtpClient();
                var persistentStorageActor = provider.GetService<PersistentStorageActorProvider>()();


                var emailActor = actorSystem.ActorOf
                (
                    EmailNotiificationActor.Create
                    (
                      smtpClient,
                      failedEmailsActor: persistentStorageActor,
                      PersistentStorageActor: persistentStorageActor
                    ).WithRouter(new RoundRobinPool(5)),
                    nameof(EmailNotiificationActor)
                );

                return () => emailActor;
            });
        }

        private static void SetupBussinessLogicActor(IServiceCollection services)
        {
            services.AddSingleton<BussinessLogicActorProvider>(provider =>
            {
                var actorSystem = provider.GetService<ActorSystem>();

                var emailSenderActor = provider.GetService<EmailSenderActorProvider>()();
                var persistentStorageActor = provider.GetService<PersistentStorageActorProvider>()();

                var unsubscribeRule = new UnsubscribeRule();
                var validateToEmailAddressRule = new ValidateToEmailAddressRule();

                unsubscribeRule.UnbsubscribedEmails.Add("test@test.com");

                IEmailBussinessRule[] emailBussinessRules =
                {
                    unsubscribeRule,
                    validateToEmailAddressRule
                };

                IBussinessRulesEvaluator rulesEvaluator = new BussinessRulesEvaluator()
                                                           .WithRules(emailBussinessRules);

                var actor = actorSystem.ActorOf
                (
                    BussinessLogicActor.Create
                    (
                      emailSendingActor: emailSenderActor,
                      failedEmailsActor: persistentStorageActor,
                      rulesEvaluator
                    ).WithRouter(new RoundRobinPool(5)),
                    nameof(BussinessLogicActor)
                );

                return () => actor;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            lifetime.ApplicationStarted.Register(() =>
            {
                // start Akka.NET
                app.ApplicationServices.GetService<ActorSystem>();
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                // stop Akka.NET
                app.ApplicationServices.GetService<ActorSystem>().Terminate().Wait();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
