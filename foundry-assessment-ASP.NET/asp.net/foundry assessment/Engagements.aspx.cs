﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using foundry_assessment.Models;
using Newtonsoft.Json;

namespace foundry_assessment
{
    public partial class Engagements : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(RunAsyncGetDataFromSource));
        }

        protected async Task RunAsyncGetDataFromSource()
        {
            using (var client = new HttpClient())
            {
                //HTTP GET call
                HttpResponseMessage response = await client.GetAsync("http://localhost:5000/engagements");
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<IList<Engagement>>(jsonString);

                    gvEngagements.DataSource = data;
                    gvEngagements.DataBind();
                }
            }
        }

        protected void InsertEngagement(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(InsertEngagementAsync));
            RegisterAsyncTask(new PageAsyncTask(RunAsyncGetDataFromSource));
        }

        protected async Task InsertEngagementAsync()
        {
            using (var client = new HttpClient()) {
                if (engagementNameAdd.Text.Length > 0 && clientIDAdd.Text.Length > 0 && employeeIDAdd.Text.Length > 0)
                {
                    // Create engagement object to POST into the backend
                    Engagement engagement = new Engagement { name = engagementNameAdd.Text, client = clientIDAdd.Text,
                        employee = employeeIDAdd.Text, description = engagementDescriptionAdd.Text };
                    var jsonInput = JsonConvert.SerializeObject(engagement);
                    var requestContent = new StringContent(jsonInput, Encoding.UTF8, "application/json");

                    // HTTP POST call
                    HttpResponseMessage response = await client.PostAsync("http://localhost:5000/engagements", requestContent);
                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.Write("Success");
                    }
                    else
                    {
                        Console.Write("Error");
                    }
                }
            }
        }

        protected void SearchEngagement(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(RunAsyncGetDataFromSourceByID));
        }

        protected async Task RunAsyncGetDataFromSourceByID()
        {
            using (var client = new HttpClient())
            {
                if (engagementIDSearch.Text.Length > 0)
                {
                    //HTTP GET call by Engagement ID
                    string apiURL = "http://localhost:5000/engagements/" + engagementIDSearch.Text;
                    HttpResponseMessage response = await client.GetAsync(apiURL);
                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = response.Content.ReadAsStringAsync().Result;
                        var data = JsonConvert.DeserializeObject<Engagement>(jsonString);
                        var dataToBind = new List<Engagement>() { data };
                        gvEngagements.DataSource = dataToBind;
                        gvEngagements.DataBind();
                    }
                }
            }
        }

        protected void OnRowEditing(object sender, GridViewEditEventArgs e)
        {
            gvEngagements.EditIndex = e.NewEditIndex;
            RegisterAsyncTask(new PageAsyncTask(RunAsyncGetDataFromSource));
        }

        protected void OnRowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvEngagements.Rows[e.RowIndex];
            Task.Run(async () => await EditEngagement(row));
            gvEngagements.EditIndex = -1;
            RegisterAsyncTask(new PageAsyncTask(RunAsyncGetDataFromSource));
        }

        protected async Task EditEngagement(GridViewRow r)
        {
            string name = (r.FindControl("txtEngagementName") as TextBox).Text;
            string description = (r.FindControl("txtEngagementDescription") as TextBox).Text;
            string id = (r.FindControl("txtEngagementID") as TextBox).Text;

            using (var client = new HttpClient())
            {
                if (name.Length > 0)
                {
                    // Create engagement object to PUT to backend
                    Engagement engagement = new Engagement { name = name, description=description };
                    var jsonInput = JsonConvert.SerializeObject(engagement);
                    var requestContent = new StringContent(jsonInput, Encoding.UTF8, "application/json");

                    // HTTP PUT call by Client ID
                    string apiURL = "http://localhost:5000/engagements/" + id;
                    HttpResponseMessage response = await client.PutAsync(apiURL, requestContent);
                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.Write("Success");
                    }
                    else
                    {
                        Console.Write("Error");
                    }
                }
            }
        }

        protected void OnRowCancellingEdit(object sender, EventArgs e)
        {
            gvEngagements.EditIndex = -1;
            RegisterAsyncTask(new PageAsyncTask(RunAsyncGetDataFromSource));
        }
    }
}