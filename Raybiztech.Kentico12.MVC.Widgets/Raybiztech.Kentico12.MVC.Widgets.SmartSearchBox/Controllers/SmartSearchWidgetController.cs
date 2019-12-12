﻿using CMS.EventLog;
using CMS.Membership;
using CMS.Search;
using Kentico.PageBuilder.Web.Mvc;
using Raybiztech.Kentico12.MVC.Widgets.SmartSearchBox.Controllers;
using Raybiztech.Kentico12.MVC.Widgets.SmartSearchBox.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

[assembly: RegisterWidget("Raybiztech.Kentico12.MVC.Widgets.SmartSearchBox", typeof(SmartSearchWidgetController), "Smart search box", Description = "Provides a text box where users can enter search expressions. Searching redirects users to a page that displays the search results. The indexes used by the search are specified by the widget displaying the results. You can configure the Smart search box to display results instantly while users type the search text.", IconClass = "icon-magnifier")]
namespace Raybiztech.Kentico12.MVC.Widgets.SmartSearchBox.Controllers
{
    public class SmartSearchWidgetController : WidgetController<SmartSearchBoxWidgetProperties>
    {

        public SmartSearchWidgetController()
        {

        }
        public SmartSearchWidgetController(IWidgetPropertiesRetriever<SmartSearchBoxWidgetProperties> propertiesRetriever,
                                        ICurrentPageRetriever currentPageRetriever) : base(propertiesRetriever, currentPageRetriever)
        {

        }
        public ActionResult Index()
        {
            SmartSearchWidgetViewModel model = new SmartSearchWidgetViewModel();
            try
            {
                var properties = GetProperties();
                model.ResultsUrl = properties.Text;
                model.PlaceHolder = properties.PlaceHolder;
                model.IndexName = properties.Index;
                model.ButtonName = properties.ButtonName;
                model.LableName = properties.LableName;
                model.LableMode = properties.ShowSearchLabel;
                model.PageSize = properties.PageSize;
                model.PageNo = "1";
                model.GroupSize = properties.GroupSize;
                TempData["GroupSize"] = model.GroupSize;
                TempData["Index"] = properties.Index;
                TempData["PageSize"] = properties.PageSize;
                var indexes = SearchIndexInfoProvider.GetSearchIndexes();
                List<SelectListItem> addList = new List<SelectListItem>();
                foreach (SearchIndexInfo index in indexes)
                {
                    addList.Add(new SelectListItem() { Text = index.IndexCodeName, Value = index.IndexCodeName, Selected = (index.IndexCodeName == model.IndexName) });
                }
                model.Indexes = addList;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("SmartSearchWidgetController", "Index", ex);

            }
            return PartialView("Widgets/SmartSearchBoxWidget/_SmartSearchBoxWidget", model);
        }

        [Route("Search/{searchresults}")]
        public ActionResult SearchResults(string searchtext, string page)
        {
            SearchResult searchResults = new SearchResult();
            SmartSearchWidgetViewModel dataList = new SmartSearchWidgetViewModel();
            try
            {
                SearchParameters searchParameters;
                int pageNo = page != null ? Convert.ToInt32(page) : 1;
                int pageSize = TempData["PageSize"] != null ? Convert.ToInt32(TempData["PageSize"].ToString()) : 10;
                string Index = TempData["Index"] != null ? TempData["Index"].ToString() : "";
                dataList.GroupSize= TempData["GroupSize"] != null ?TempData["GroupSize"].ToString(): "4";
                dataList.SearchText = searchtext;
                TempData.Keep();
                dataList.PageNo = Convert.ToString(pageNo);
                dataList.PageSize = Convert.ToString(pageSize);
               
                searchParameters = SearchParameters.PrepareForPages(searchtext, new[] { Index }, pageNo, pageSize, MembershipContext.AuthenticatedUser);
                searchResults = SearchHelper.Search(searchParameters);
                dataList.TotalResultCount = searchResults.TotalNumberOfResults;
                Pager pager = new Pager(dataList.TotalResultCount, pageNo, Convert.ToInt32(dataList.PageSize) , Convert.ToInt32(dataList.GroupSize));
                dataList.Pager = pager;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("SmartSearchWidgetController", "SearchResults", ex);
            }
            dataList.Items = searchResults.Items;
            return View("Widgets/SmartSearchBoxWidget/_SmartSearchResultWidget", dataList);
        }
        public ActionResult AssignValues(string index,string groupsize,string pagesize)
        {
            bool status = false;
            try
            {
                TempData["Index"] = index;
                TempData["GroupSize"] = groupsize;
                TempData["PageSize"] = pagesize;
                TempData.Keep();
                status = true;
            }
            catch (Exception ex)
            {

                EventLogProvider.LogException("SmartSearchWidgetController", "AssignValues", ex);
            }

            return Json(status, JsonRequestBehavior.AllowGet);
        }

    }
}
