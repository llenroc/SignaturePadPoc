﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SignaturePadPoc.Common;
using SignaturePadPoc.DAL;
using SignaturePadPoc.DAL.Models;
using SignaturePadPoc.Entities;
using Xamarin.Forms;

namespace SignaturePadPoc.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly ObservableCollection<DocumentEntity> _documentEntities = new ObservableCollection<DocumentEntity>();

        public MainPage()
        {
            InitializeComponent();
            ListView.ItemsSource = _documentEntities;
            ListView.RefreshCommand = new Command(async t =>
            {
                await RefreshListDataAsync();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshListDataAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _documentEntities.Clear();
        }

        private async Task RefreshListDataAsync()
        {
            ListView.IsRefreshing = true;

            var userDocuments = (await RepositoryManager.UserDocumentRepositoryInstance.GetAsync(x => x.AssignedUserId == ApplicationContext.LoggedInUserId && x.IsCompleted == false))?.Select(x => x.DocumentId).ToList();

            if (userDocuments?.Count > 0)
            {
                Reset((await RepositoryManager.DocumentRepositoryInstance.GetAsync(x => userDocuments.Any(y => y == x.DocumentId)))?.ToList());
            }
            else
            {
                _documentEntities.Clear();
            }

            ListView.IsRefreshing = false;
        }

        private void Reset(IReadOnlyCollection<Document> documents)
        {
            _documentEntities.Clear();
            if (!(documents?.Count > 0))
            {
                return;
            }

            foreach (var document in documents)
            {
                _documentEntities.Add(new DocumentEntity
                {
                    Url = document.DocumentUrl,
                    Id = document.DocumentId,
                    Title = document.Title,
                    SubTitle = document.Description
                });
            }
        }

        private async void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e?.SelectedItem == null)
            {
                return;
            }

            var selectedDocument = e.SelectedItem as DocumentEntity;

            if (selectedDocument != null)
            {
                await Navigation.PushAsync(new DocumentPage(selectedDocument));
            }

            ListView.SelectedItem = null;
        }

        private async void Button_OnClicked(object sender, EventArgs e) => await Navigation.PushAsync(new AddDocumentPage());

        private async void MenuItem_OnClicked(object sender, EventArgs e)
        {
            await RepositoryManager.UserDocumentRepositoryInstance.ForceSyncAsync();
            await RepositoryManager.DocumentRepositoryInstance.ForceSyncAsync();
            await RefreshListDataAsync();
        }
    }
}
