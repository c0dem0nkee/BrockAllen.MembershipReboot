﻿using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace RolesAdmin.Controllers
{
    public class GroupIndexViewModel
    {
        public IEnumerable<GroupViewModel> Groups { get; set; }
        public IEnumerable<SelectListItem> GroupsAsList
        {
            get
            {
                return Groups.Select(x=>new SelectListItem { Text=x.Name, Value=x.Id.ToString() });
            }
        }
    }

    public class GroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<GroupViewModel> Children { get; set; }
    }

    public class HomeController : Controller
    {
        GroupService groupSvc;
        public HomeController(GroupService groupSvc)
        {
            this.groupSvc = groupSvc;
        }

        public ActionResult Index()
        {
            var list = new List<GroupViewModel>();
            foreach(var item in groupSvc.GetAll())
            {
                var kids = new List<GroupViewModel>();
                foreach (var child in item.Children)
                {
                    var childGrp = groupSvc.Get(child.ChildGroupId);
                    kids.Add(new GroupViewModel { Id = child.ChildGroupId, Name = childGrp.Name });
                }
                var gvm = new GroupViewModel
                {
                    Id = item.Id, Name = item.Name,
                    Children = kids
                };
                list.Add(gvm);
            }
            var vm = new GroupIndexViewModel { 
                Groups = list
            };
            return View("Index", vm);
        }

        [HttpPost]
        public ActionResult Add(string name)
        {
            try
            {
                groupSvc.Create(name);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                groupSvc.Delete(id);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }

        [HttpPost]
        public ActionResult ChangeName(int id, string name)
        {
            try
            {
                groupSvc.ChangeName(id, name);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }

        [HttpPost]
        public ActionResult AddChild(int id, int child)
        {
            try
            {
                groupSvc.AddChildGroup(id, child);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }
        [HttpPost]
        public ActionResult RemoveChild(int id, int child)
        {
            try
            {
                groupSvc.RemoveChildGroup(id, child);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }
    }
}
