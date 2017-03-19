﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitBucket.REST.API.Helpers;
using BitBucket.REST.API.Interfaces;
using BitBucket.REST.API.Mappings;
using BitBucket.REST.API.Models.Standard;
using BitBucket.REST.API.QueryBuilders;
using BitBucket.REST.API.Wrappers;
using ParseDiff;
using RestSharp;

namespace BitBucket.REST.API.Clients.Standard
{
    public class PullRequestsClient : ApiClient, IPullRequestsClient
    {
        private readonly BitbucketRestClient _internalRestClient;
        private readonly BitbucketRestClient _webClient;

        public PullRequestsClient(BitbucketRestClient restClient, BitbucketRestClient internalRestClient, BitbucketRestClient webClient, Connection connection) : base(restClient, connection)
        {
            _internalRestClient = internalRestClient;
            _webClient = webClient;
        }

        public async Task<IEnumerable<PullRequest>> GetAllPullRequests(string repositoryName, string ownerName)
        {
            var url = ApiUrls.PullRequestsAllStates(ownerName, repositoryName);
            return await RestClient.GetAllPages<PullRequest>(url, 100);
        }


        public async Task<IEnumerable<UserShort>> GetAuthors(string repositoryName, string ownerName)
        {
            var url = ApiUrls.PullRequestsAuthors(ownerName, repositoryName);
            return await _internalRestClient.GetAllPages<UserShort>(url, 100);
        }

        public async Task<IteratorBasedPage<PullRequest>> GetPullRequestsPage(string repositoryName, string ownerName, int limit = 20, int page = 1, IQueryConnector query = null)
        {
            var url = ApiUrls.PullRequestsAllStates(ownerName, repositoryName);
            var request = new BitbucketRestRequest(url, Method.GET);
            request.AddQueryParameter("pagelen", limit.ToString());
            request.AddQueryParameter("page", page.ToString());
            if (query != null)
            {
                request.AddQueryParameter("q", query.Build());
            }
            var response = await RestClient.ExecuteTaskAsync<IteratorBasedPage<PullRequest>>(request);
            return response.Data;
        }

        public async Task<IEnumerable<FileDiff>> GetPullRequestDiff(string repositoryName, long id)
        {
            return await GetPullRequestDiff(repositoryName, Connection.Credentials.Login, id);
        }

        public async Task<IEnumerable<FileDiff>> GetPullRequestDiff(string repositoryName, string owner, long id)
        {
            var url = ApiUrls.PullRequestDiff(owner, repositoryName, id);
            var request = new BitbucketRestRequest(url, Method.GET);
            var response = await RestClient.ExecuteTaskAsync(request);
            return DiffFileParser.Parse(response.Content);
        }

        public async Task<Participant> ApprovePullRequest(string repositoryName, long id)
        {
            return await ApprovePullRequest(repositoryName, Connection.Credentials.Login, id);
        }

        public async Task<Participant> ApprovePullRequest(string repositoryName, string ownerName, long id)
        {
            var url = ApiUrls.PullRequestApprove(ownerName, repositoryName, id);
            var request = new BitbucketRestRequest(url, Method.POST);
            var response = await RestClient.ExecuteTaskAsync<Participant>(request);
            return response.Data;
        }

        public async Task DisapprovePullRequest(string repositoryName, long id)
        {
            await DisapprovePullRequest(repositoryName, Connection.Credentials.Login, id);
        }

        public async Task DisapprovePullRequest(string repositoryName, string ownerName, long id)
        {
            var url = ApiUrls.PullRequestApprove(ownerName, repositoryName, id);
            var request = new BitbucketRestRequest(url, Method.DELETE);
            await RestClient.ExecuteTaskAsync(request);
        }

        public async Task<IEnumerable<Commit>> GetPullRequestCommits(string repositoryName, string ownerName, long id)
        {
            var url = ApiUrls.PullRequestCommits(ownerName, repositoryName, id);
            var commits = await RestClient.GetAllPages<Commit>(url);
            foreach (var commit in commits)
            {
                commit.CommitHref = $"{Connection.MainUrl}{ownerName}/{repositoryName}/commits/{commit.Hash}";
            }
            return commits;
        }

        public async Task<IEnumerable<Comment>> GetPullRequestComments(string repositoryName, long id)
        {
            return await GetPullRequestComments(repositoryName, Connection.Credentials.Login, id);
        }

        public async Task<IEnumerable<Comment>> GetPullRequestComments(string repositoryName, string ownerName, long id)
        {
            var url = ApiUrls.PullRequestComments(ownerName, repositoryName, id);
            return await RestClient.GetAllPages<Comment>(url);
        }

        public async Task<PullRequest> GetPullRequest(string repositoryName, string owner, long id)
        {
            var url = ApiUrls.PullRequest(owner, repositoryName, id);
            var request = new BitbucketRestRequest(url, Method.GET);
            var response = await RestClient.ExecuteTaskAsync<PullRequest>(request);
            return response.Data;
        }

        public async Task<PullRequest> GetPullRequestForBranches(string repositoryName, string ownerName, string sourceBranch, string destBranch)
        {
            var url = ApiUrls.PullRequests(ownerName, repositoryName);

            var queryBuilderString = new QueryBuilder()
                .Add("source.branch.name", sourceBranch)
                .And()
                .Add("destination.branch.name", destBranch)
                .And()
                .State(PullRequestOptions.OPEN)
                .Build();

            var query = new QueryString()
            {
                {"q", queryBuilderString},
            };
            var response = await RestClient.GetAllPages<PullRequest>(url, query: query, limit: 20);
            return response.SingleOrDefault();
        }

        public Task<IEnumerable<FileDiff>> GetCommitsDiff(string repoName, string owner, string fromCommit, string toCommit)
        {
            throw new System.NotImplementedException();
        }

        public async Task CreatePullRequest(PullRequest pullRequest, string repositoryName, string owner)
        {
            pullRequest.Author = new User()
            {
                Username = Connection.Credentials.Login
            };

            var url = ApiUrls.PullRequests(owner, repositoryName);
            var request = new BitbucketRestRequest(url, Method.POST);
            request.AddParameter("application/json; charset=utf-8", request.JsonSerializer.Serialize(pullRequest), ParameterType.RequestBody);
            var response = await RestClient.ExecuteTaskAsync<PullRequest>(request);
        }

        public async Task<IEnumerable<UserShort>> GetRepositoryUsers(string repositoryName, string ownerName, string filter)
        {
            var url = ApiUrls.Mentions(ownerName, repositoryName);
            var query = new QueryString()
            {
                {"term",filter }
            };

            var request = new BitbucketRestRequest(url, Method.GET);

            foreach (var par in query)
                request.AddQueryParameter(par.Key, par.Value);

            var response = await _webClient.ExecuteTaskAsync<List<UserShort>>(request);
            return response.Data;
        }
    }
}