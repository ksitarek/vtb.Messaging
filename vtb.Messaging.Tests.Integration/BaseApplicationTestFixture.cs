using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace vtb.Messaging.Tests.Integration
{
    [SetUpFixture]
    public class BaseApplicationTestFixture : IDisposable
    {
        public static EventWaitHandle MainWaitHandle;
        private readonly ContainerHelper _containerHelper = new ContainerHelper();

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            MainWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            await _containerHelper.StartContainers();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _containerHelper.StopContainers();
        }

        public void Dispose()
        {
            OneTimeTearDown().GetAwaiter().GetResult();
            MainWaitHandle.Dispose();
        }
    }
}