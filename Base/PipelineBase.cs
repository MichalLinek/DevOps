using System;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;

namespace AzureDevops.Base
{
    public abstract class PipelineBase
    {
        protected TeamProjectReference GetProjectReference(Guid projectId)
        {  
            var connection = GetConnection(projectId);
            var projectClient = connection.GetClient<ProjectHttpClient>();

            var project = projectClient.GetProject(projectId.ToString()).Result;

            if (project == null) {
                throw new Exception("Project not found");
            }

            return project;
        }

        protected VssConnection GetConnection(Guid projectId)
        {
            var project = GetProjectReference(projectId);
            var creds = new VssClientCredentials();
            return new VssConnection(new Uri(Program.CollectionUrl), creds);
        }
    }
}
