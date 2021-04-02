using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;
using vtb.Messaging.Tests.Integration.TestClasses.Messages;

namespace vtb.Messaging.Tests.Integration.MessageConsumption
{
    public class CommandConsumerTests : BaseConsumerTests
    {
        [Test]
        public void Will_Invoke_Consumer_Handle_Method_When_Message_Arrives()
        {
            // Given
            var message = new TestCommand1()
            {
                Id = Guid.NewGuid()
            };

            // When
            PublishMessage(message);

            // Then
            WaitMany(1);
            var sb = _factory.Services.GetRequiredService<StringBuilder>();
            var sbhc = sb.GetHashCode();
            sb.ToString().ShouldBe($"Handle: {nameof(TestCommand1)} with ID:{message.Id} on behalf of {Guid.Empty}{Environment.NewLine}");
        }

        [Test]
        public void Will_Keep_Handlers_In_Separate_Container_Scope()
        {
            // Given
            var messages = new Dictionary<Guid, TestCommand1>();
            for (var i = 0; i < 1000; i++)
            {
                messages.Add(Guid.NewGuid(), new TestCommand1() { Id = Guid.NewGuid() });
            }

            // When
            Parallel.ForEach(messages, (kvp) =>
            {
                PublishMessage(kvp.Value, new Dictionary<string, object>()
                {
                    ["tenantId"] = kvp.Key.ToString()
                });
            });

            // Then
            WaitMany(messages.Count);
            var sb = _factory.Services.GetRequiredService<StringBuilder>();
            var allString = sb.ToString();
            var lines = allString.Split(Environment.NewLine).ToList();
            foreach (var message in messages)
            {
                var expectedLine = $"Handle: {nameof(TestCommand1)} with ID:{message.Value.Id} on behalf of {message.Key}";
                var foundLine = lines.FirstOrDefault(x => x == expectedLine);
                foundLine.ShouldNotBeNull();

                lines.Remove(foundLine);
            }
        }

        private void PublishMessage(TestCommand1 message, Dictionary<string, object> headers = null)
        {
            var messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            var basicProperties = _client.CreateBasicProperties();
            basicProperties.Headers = headers;

            _client.BasicPublish(
                message.GetType().FullName,
                "*",
                false,
                basicProperties,
                messageBytes);
        }

        private void WaitMany(int cnt)
        {
            while (cnt > 0)
            {
                BaseApplicationTestFixture.MainWaitHandle.WaitOne();
                cnt--;
            }
        }
    }
}