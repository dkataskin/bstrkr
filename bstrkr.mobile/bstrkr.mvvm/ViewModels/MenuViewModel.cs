﻿using System;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class MenuViewModel : MvxViewModel
	{
		private string _title;
		private MenuSection _section;

		public string Title
		{
			get { return _title; }
			set
			{
				if (!string.Equals(_title, value))
				{
					_title = value;
					this.RaisePropertyChanged(() => this.Title);
				}
			}
		}

		public MenuSection Section
		{ 
			get { return _section; }
			set
			{
				if (_section != value)
				{
					_section = value;
					this.RaisePropertyChanged(() => this.Section);
				}
			}
		}
	}
}