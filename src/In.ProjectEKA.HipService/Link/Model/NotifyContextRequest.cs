using System.Collections.Generic;
using In.ProjectEKA.HipService.Common.Model;

namespace In.ProjectEKA.HipService.Link.Model
{
    public class NotifyContextRequest
    {
        public string PatientId { get; }

        public string PatientReference { get; }

        public string CareContextReference { get; }

        public List<string> HiTypes { get; }
        
        public string HipId { get; }


        public NotifyContextRequest(string patientId, string patientReference, string careContextReference,
            List<string> hiTypes, string hipId)
        {
            PatientId = patientId;
            PatientReference = patientReference;
            CareContextReference = careContextReference;
            HiTypes = hiTypes;
            HipId = hipId;
        }
    }
}