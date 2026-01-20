using Microsoft.AspNetCore.Mvc;
using OrchardCore.Demo.Models;
using OrchardCore.Demo.ViewModels;
using YesSql;

namespace OrchardCore.Demo.Controllers;

public sealed class TodoController : Controller
{
    private readonly ISession _session;
    private readonly Entities.IIdGenerator _idGenerator;

    public TodoController(ISession session, Entities.IIdGenerator idGenerator)
    {
        _session = session;
        _idGenerator = idGenerator;
    }

    public async Task<IActionResult> Index()
    {
        var list = (await _session.Query<TodoModel>().ListAsync())
            .Select(m => new TodoViewModel()
            {
                TodoId = m.TodoId,
                Text = m.Text,
                DueDate = m.DueDate,
                IsCompleted = m.IsCompleted,
            })
            .ToList();

        return View(list);
    }

    public IActionResult Create()
    {
        var viewModel = new TodoViewModel
        {
            TodoId = _idGenerator.GenerateUniqueId(),
            DisplayMode = "Edit",
        };

        return View(nameof(Edit), viewModel);
    }

    public async Task<IActionResult> Edit(string todoId)
    {
        var model = (await _session.Query<TodoModel>().ListAsync())
            .FirstOrDefault(m => m.TodoId == todoId);

        if (model == null)
        {
            return NotFound();
        }

        var viewModel = new TodoViewModel()
        {
            TodoId = model.TodoId,
            Text = model.Text,
            DueDate = model.DueDate,
            IsCompleted = model.IsCompleted,
            DisplayMode = "Edit",
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Update(TodoViewModel viewModel, string returnUrl = "")
    {
        if (ModelState.IsValid)
        {
            var model = (await _session.Query<TodoModel>().ListAsync())
                .FirstOrDefault(m => m.TodoId == viewModel.TodoId);

            model ??= new TodoModel() { TodoId = viewModel.TodoId };

            model.Text = viewModel.Text;
            model.DueDate = viewModel.DueDate;
            model.IsCompleted = viewModel.IsCompleted;

            await _session.SaveAsync(model);

            if (Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl, true);
            }

            return RedirectToAction(nameof(Index), "Todo");
        }

        viewModel.DisplayMode = "Edit";
        return View(nameof(Edit), viewModel);
    }

    public async Task<IActionResult> Delete(string todoId)
    {
        var model = (await _session.Query<TodoModel>().ListAsync())
            .FirstOrDefault(m => m.TodoId == todoId);

        if (model == null)
        {
            return NotFound();
        }

        _session.Delete(model);

        return RedirectToAction(nameof(Index), "Todo");
    }
}
