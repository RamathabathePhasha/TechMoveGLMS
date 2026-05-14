using Xunit;

namespace TechMoveGLMS.Tests
{
    public class ContractWorkflowTests
    {
        private bool CanCreateServiceRequest(string status)
        {
            // Cannot create if Expired or OnHold
            return status != "Expired" && status != "OnHold";
        }

        [Fact]
        public void ActiveContract_AllowsRequest()
        {
            Assert.True(CanCreateServiceRequest("Active"));
        }

        [Fact]
        public void DraftContract_AllowsRequest()
        {
            Assert.True(CanCreateServiceRequest("Draft"));
        }

        [Fact]
        public void ExpiredContract_BlocksRequest()
        {
            Assert.False(CanCreateServiceRequest("Expired"));
        }

        [Fact]
        public void OnHoldContract_BlocksRequest()
        {
            Assert.False(CanCreateServiceRequest("OnHold"));
        }
    }
}