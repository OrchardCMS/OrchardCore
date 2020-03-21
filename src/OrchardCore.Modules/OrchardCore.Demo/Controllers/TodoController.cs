using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.Demo.Models;
using OrchardCore.Demo.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.Demo.Controllers
{
    public class TodoController : Controller
    {
        ISession _session;
        public TodoController(
            ISession session)
        {
            _session = session;
        }
        public async Task<IActionResult> Index()
        {
            var list = (await _session.Query<TodoModel>().ListAsync()).ToList();
            return View(list);
        }

        public IActionResult Create()
        {
            var instance = new TodoModel();
            instance.DisplayMode = "Edit";
            return View("Edit", instance);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var list = (await _session.Query<TodoModel>().ListAsync()).ToList();
            var storeModel = list.Where(c => c.Id == id).FirstOrDefault();
            if (storeModel != null)
            {
                storeModel.DisplayMode = "Edit";
                return View(storeModel);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(TodoModel model, string returnUrl = "")
        {
            if (ModelState.IsValid)
            {
                var list = (await _session.Query<TodoModel>().ListAsync()).ToList();
                var storeModel = list.Where(c => c.Id == model.Id).FirstOrDefault();
                if (storeModel == null)
                {
                    if (list.Count == 0)
                    {
                        model.Id = "1";
                    }
                    else
                    {
                        model.Id = (list.Select(c => int.Parse(c.Id)).Max() + 1).ToString();
                    }

                    storeModel = model;
                }
                else
                {
                    storeModel.Text = model.Text;
                    storeModel.DueDate = model.DueDate;
                    storeModel.IsCompleted = model.IsCompleted;
                }
                _session.Save(storeModel);
                
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Todo");
                }
            }
            else
            {
                return View(model);
            }
        }


        public async Task<IActionResult> Delete(string id)
        {
            var list = (await _session.Query<TodoModel>().ListAsync()).ToList();
            var storeModel = list.Where(c => c.Id == id).FirstOrDefault();
            if (storeModel != null)
            {
                _session.Delete(storeModel);

                return RedirectToAction("Index", "Todo");
            }
            else
            {
                return NotFound();
            }

        }
    }
}
