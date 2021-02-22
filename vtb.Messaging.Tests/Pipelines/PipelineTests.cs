using System;
using System.Threading.Tasks;
using NUnit.Framework;
using vtb.Messaging.Context;
using vtb.Messaging.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Text;

namespace vtb.Messaging.Tests.Pipelines
{
    public class PipelineTests
    {
        private ServiceProvider _serviceProvider;
        private StringBuilder _sb;

        [SetUp]
        public void SetUp()
        {
            _sb = new StringBuilder();

            var services = new ServiceCollection();
            services.AddScoped<TestFilter1>();
            services.AddScoped<TestFilter2>();
            services.AddScoped<TestFilter3>();
            services.AddScoped<BreakingFilter>();
            services.AddScoped(_ => _sb);

            _serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public void Will_Run_All_Filters()
        {
            var pipeline = PipelineBuilder.Create()
                .AddFilter<TestFilter1>()
                .AddFilter<TestFilter2>()
                .AddFilter<TestFilter3>()
                .Build(_serviceProvider);

            var mc = TenantMessageContextBuilder<TestMessage>.CreateForMessage(new TestMessage(Guid.NewGuid()))
                .WithNewId()
                .WithUserId(Guid.Empty)
                .WithTenant(Guid.Empty)
                .Build();

            pipeline.Run(mc, _ =>
            {
                _sb.AppendLine("Last");
                return Task.CompletedTask;
            });

            mc.GetParam<bool>("T1").ShouldBeTrue();
            mc.GetParam<bool>("T2").ShouldBeTrue();
            mc.GetParam<bool>("T3").ShouldBeTrue();

            var testString = _sb.ToString().Split(Environment.NewLine);
            testString[0].ShouldBe("Start T1");
            testString[1].ShouldBe("Start T2");
            testString[2].ShouldBe("Start T3");
            testString[3].ShouldBe("Last");
            testString[4].ShouldBe("End T3");
            testString[5].ShouldBe("End T2");
            testString[6].ShouldBe("End T1");
        }

        [Test]
        public void BreakingFilter_Will_Break_Execution()
        {
            var pipeline = PipelineBuilder.Create()
                .AddFilter<TestFilter1>()
                .AddFilter<BreakingFilter>()
                .AddFilter<TestFilter3>()
                .Build(_serviceProvider);

            var mc = TenantMessageContextBuilder<TestMessage>.CreateForMessage(new TestMessage(Guid.NewGuid()))
                .WithNewId()
                .WithUserId(Guid.Empty)
                .WithTenant(Guid.Empty)
                .Build();

            pipeline.Run(mc, _ => Task.CompletedTask);

            mc.GetParam<bool>("T1").ShouldBeTrue();
            mc.GetParam<bool>("T3").ShouldBeFalse();
        }

        public class TestMessage
        {
            public TestMessage(Guid val)
            {
                Val = val;
            }

            public Guid Val { get; }
        }
        public class TestFilter1 : IFilter
        {
            private readonly StringBuilder _sb;

            public TestFilter1(StringBuilder sb)
            {
                _sb = sb;
            }

            public Task Invoke(IMessageContext messageContext, FilterDelegate next)
            {
                messageContext.SetParam("T1", true);
                _sb.AppendLine("Start T1");
                var task = next(messageContext);
                _sb.AppendLine("End T1");

                return task;
            }
        }
        public class TestFilter2 : IFilter
        {
            private readonly StringBuilder _sb;

            public TestFilter2(StringBuilder sb)
            {
                _sb = sb;
            }

            public Task Invoke(IMessageContext messageContext, FilterDelegate next)
            {
                messageContext.SetParam("T2", true);
                _sb.AppendLine("Start T2");
                var task = next(messageContext);
                _sb.AppendLine("End T2");

                return task;
            }
        }
        public class TestFilter3 : IFilter
        {
            private readonly StringBuilder _sb;

            public TestFilter3(StringBuilder sb)
            {
                _sb = sb;
            }

            public Task Invoke(IMessageContext messageContext, FilterDelegate next)
            {
                messageContext.SetParam("T3", true);
                _sb.AppendLine("Start T3");
                var task = next(messageContext);
                _sb.AppendLine("End T3");

                return task;
            }
        }
        public class BreakingFilter : IFilter
        {
            public Task Invoke(IMessageContext messageContext, FilterDelegate next)
            {
                return Task.CompletedTask;
            }
        }
    }
}
