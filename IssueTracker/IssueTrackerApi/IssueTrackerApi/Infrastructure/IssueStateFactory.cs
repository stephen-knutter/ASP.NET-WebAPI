using System;
using IssueTrackerApi.Models;

namespace IssueTrackerApi.Infrastructure
{
    public class IssueStateFactory : IStateFactory<Issue, IssueState> // <1>
    {
        private readonly IssueLinkFactory _links;

        public IssueStateFactory(IssueLinkFactory links)
        {
            _links = links;
        }

        public IssueState Create(Issue issue)
        {
            var model = new IssueState // <2>
            {
                Id = issue.Id,
                Title = issue.Title,
                Description = issue.Description,
                Status = issue.Status //Enum.GetName(typeof(IssueStatus), issue.Status)
            };

            model.Links.Add(_links.Self(issue.Id)); // <2>
            model.Links.Add(_links.Transition(issue.Id));

            switch (issue.Status) // <3>
            {
                case IssueStatus.Closed:
                    model.Links.Add(_links.Open(issue.Id));
                    break;
                case IssueStatus.Open:
                    model.Links.Add(_links.Close(issue.Id));
                    break;
            }
            return model;
        }
    }
}