using Refit;
using SmallEco.DTO;

namespace SmallEco.Client.RefitClient;

public interface ITodoApi
{
  [Post("/api/Todo/QryDataList")]
  [Headers("Authorization: Bearer")]
  Task<List<TodoDto>> QryDataListAsync(TodoQryAgs args);

  [Post("/api/Todo/AddFormData")]
  [Headers("Authorization: Bearer")]
  Task<TodoDto> AddFormDataAsync(string newTodoDesc);
}
