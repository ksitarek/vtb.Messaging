using System;
using System.Threading.Tasks;
using vtb.Messaging.Context;
using vtb.Messaging.Pipelines;

namespace vtb.Messaging.Tests.Integration.TestClasses.Filters
{
    public class TestTenantFilter : IFilter
    {
        private readonly TenantProvider _tenantProvider;

        public TestTenantFilter(TenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        public Task Invoke(IMessageContext messageContext, FilterDelegate next)
        {
            if (_tenantProvider.TenantId == Guid.Empty)
            {
                _tenantProvider.TenantId = messageContext.TenantId;
            }

            return next(messageContext);
        }

        public class TenantProvider
        {
            public Guid TenantId { get; set; } = Guid.Empty;
        }
    }
}