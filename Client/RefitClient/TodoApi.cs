﻿using Refit;
using SmallEco.DTO;

namespace SmallEco.Client.RefitClient;

public interface ITodoApi
{
  [Post("/api/Todo/QryDataList")]
  Task<List<TodoDto>> QryDataListAsync(TodoQryAgs args);

  [Post("/api/Todo/AddFormData")]
  Task<TodoDto> AddFormDataAsync(string newTodoDesc);
}
