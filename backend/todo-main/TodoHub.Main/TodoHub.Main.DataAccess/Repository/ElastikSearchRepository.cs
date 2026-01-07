using AutoMapper;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.DataAccess.Context;

namespace TodoHub.Main.DataAccess.Repository
{
    public class ElastikSearchRepository : IElastikSearchRepository
    {
        private readonly ElasticsearchClient _client;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        public ElastikSearchRepository(ElasticsearchClient client, IMapper mapper, ApplicationDbContext context)  
        {
            _client = client;
            _mapper = mapper;
            _context = context;
        }
        // Create, Update Document in Elastik Search
        public async Task<bool> UpsertDocRepo(TodoDTO todo, Guid todoId, CancellationToken ct)
        {
            try
            {
                Log.Information("UpsertDocRepo starting in ESR");

                var entity = _mapper.Map<SearchTodoDTO>(todo);
                var res = await _client.IndexAsync(entity, x => x.Index("todos").Id(todoId), ct);

                Log.Information("ES response valid: {IsValid}", res.IsValidResponse);
                Log.Information("ES debug: {Debug}", res.DebugInformation);

                return res.IsValidResponse;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpsertDocRepo failed");
                return false;
            }
        }

        // Create Index 
        public async Task<bool> CreateIndexRepo()
        {
            try
            {
                Log.Information("CreateIndexRepo starting in ESR");
                var exists = await _client.Indices.ExistsAsync("todos");
                if (exists.Exists)
                {
                    var res = await _client.Indices.DeleteAsync("todos");
                    Log.Information($"Response in CreateIndexRepo().DeleteAsync: {res.DebugInformation}");
                }
                var response = await _client.Indices.CreateAsync("todos");
                Log.Information("ES response valid: {IsValid}", response.IsValidResponse);
                Log.Information($"Response in CreateIndexRepo().CreateAsync: {response.DebugInformation}");
                return response.IsValidResponse;
            }
            catch (Exception ex) 
            {
                Log.Error(ex, "CreateIndexRepo failed");
                return false;
            }
        }

        // Delete Document in Eleastik Search
        public async Task<bool> DeleteDocRepo(Guid TodoId, Guid OwnerId, CancellationToken ct)
        {
            try
            {
                Log.Information("DeleteDocRepo starting in ESR");
                var get = await _client.GetAsync<SearchTodoDTO>(TodoId, g => g.Index("todos"),ct);

                if (!get.Found) return false;
                if (get.Source!.OwnerId != OwnerId) return false;

                var response = await _client.DeleteAsync<SearchTodoDTO>(TodoId, x => x.Index("todos"),ct);
                Log.Information("ES response valid: {IsValid}", response.IsValidResponse);
                Log.Information($"Response in DeleteDocRepo(): {response.DebugInformation}");
                return response.IsValidResponse;
            } catch (Exception ex)
            {
                Log.Error(ex, "DeleteDocRepo failed");
                return false;
            }
        }

        // Reindexing
        public async Task<bool> ReIndexRepo()
        {
            try
            {
                Log.Information("ReIndexRepo starting in ESR");
                var todos = await _context.Todos.ToListAsync();
                var docs = _mapper.Map<List<SearchTodoDTO>>(todos);


                var bulk = new BulkRequest("todos")
                {
                    Operations = new List<IBulkOperation>()
                };

                foreach (var doc in docs)
                {
                    bulk.Operations.Add(new BulkIndexOperation<SearchTodoDTO>(doc)
                    {
                        Id = doc.Id.ToString()

                    });
                }

                var res = await _client.BulkAsync(bulk);

                return res.IsValidResponse;
            } catch (Exception ex)
            {
                Log.Error(ex, "ReIndexRepo failed");
                return false;
            }
        }

        // Main Search 
        public async Task<List<TodoDTO>> SearchDocumentsRepo(Guid userId, string query, CancellationToken ct)
        {
            try
            {
                Log.Information("SearchDocumentsRepo starting in ESR");
                query = query?.Trim() ?? "";

                var response = await _client.SearchAsync<SearchTodoDTO>(s => s
                    .Index("todos")
                    .Size(50)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f.Term(t => t
                                .Field(new Field("ownerId.keyword"))
                                .Value(userId.ToString())
                            ))
                            .Should(
                                sh => sh.MultiMatch(mm => mm
                                    .Query(query)
                                    .Fields(new[] { "title", "description" })
                                    .Fuzziness(new Fuzziness(1))
                                    .Operator(Operator.Or)
                                ),
                                sh => sh.Prefix(p => p.Field(new Field("title")).Value(query.ToLower())),
                                sh => sh.Prefix(p => p.Field(new Field("description")).Value(query.ToLower())),
                                sh => sh.Wildcard(w => w.Field(new Field("title")).Value($"*{query.ToLower()}*")),
                                sh => sh.Wildcard(w => w.Field(new Field("description")).Value($"*{query.ToLower()}*"))
                            )
                            .MinimumShouldMatch(1)
                        )
                    ), ct
                );


                Log.Information("Search query='{Query}', userId={UserId}, returned={ReturnedCount}",
                    query, userId, response.Documents.Count);
                Log.Information("ES response valid: {IsValid}", response.IsValidResponse);
                Log.Information($"Response in SearchDocumentsRepo(): {response.DebugInformation}");

                if (!response.IsValidResponse) {
                    return new List<TodoDTO>();
                }
                var output = _mapper.Map<List<TodoDTO>>(response.Documents.ToList());
                return output;
            } catch (Exception ex)
            {
                Log.Error(ex, "SearchDocumentsRepo failed");
                return new List<TodoDTO>();
            }
        }
    }
}
