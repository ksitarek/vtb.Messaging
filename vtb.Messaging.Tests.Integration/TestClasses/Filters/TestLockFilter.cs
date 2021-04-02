using System.Threading.Tasks;
using vtb.Messaging.Context;
using vtb.Messaging.Pipelines;

namespace vtb.Messaging.Tests.Integration.TestClasses.Filters
{
    public class TestLockFilter : IFilter
    {
        public async Task Invoke(IMessageContext messageContext, FilterDelegate next)
        {
            await next(messageContext);

            BaseApplicationTestFixture.MainWaitHandle.Set();
        }
    }
}