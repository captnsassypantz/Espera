﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using Espera.Core;
using FlagLib.Patterns.MVVM;

namespace Espera.View
{
    internal class MainViewModel : ViewModelBase<MainViewModel>, IDisposable
    {
        private readonly Library library;
        private string selectedArtist;
        private bool isAdding;
        private string currentAddingPath;
        private Song selectedSong;
        private Song selectedPlaylistSong;
        private readonly Timer updateTimer;

        public IEnumerable<string> Artists
        {
            get
            {
                // If we are currently adding songs, copy the songs to a new list, so that we don't run into performance issues
                IEnumerable<Song> songs = this.IsAdding ? this.library.Songs.ToList() : this.library.Songs;

                return songs
                    .GroupBy(song => song.Artist)
                    .Select(group => group.Key)
                    .OrderBy(artist => artist);
            }
        }

        public string SelectedArtist
        {
            get { return this.selectedArtist; }
            set
            {
                if (this.SelectedArtist != value)
                {
                    this.selectedArtist = value;
                    this.OnPropertyChanged(vm => vm.SelectedArtist);
                    this.OnPropertyChanged(vm => vm.SelectableSongs);
                }
            }
        }

        public IEnumerable<Song> SelectableSongs
        {
            get
            {
                return this.library.Songs
                    .Where(song => song.Artist == this.SelectedArtist);
            }
        }

        public Song SelectedSong
        {
            get { return this.selectedSong; }
            set
            {
                if (this.SelectedSong != value)
                {
                    this.selectedSong = value;
                    this.OnPropertyChanged(vm => vm.SelectedSong);
                }
            }
        }

        public Song SelectedPlaylistSong
        {
            get { return this.selectedPlaylistSong; }
            set
            {
                if (this.SelectedPlaylistSong != value)
                {
                    this.selectedPlaylistSong = value;
                    this.OnPropertyChanged(vm => vm.SelectedPlaylistSong);
                }
            }
        }

        public IEnumerable<Song> Playlist
        {
            get { return this.library.Playlist; }
        }

        public double Volume
        {
            get { return this.library.Volume; }
            set
            {
                this.library.Volume = (float)value;
                this.OnPropertyChanged(vm => vm.Volume);
            }
        }

        public TimeSpan TotalTime
        {
            get { return this.library.TotalTime; }
        }

        public int TotalSeconds
        {
            get { return (int)this.TotalTime.TotalSeconds; }
        }

        public TimeSpan CurrentTime
        {
            get { return this.library.CurrentTime; }
        }

        public int CurrentSeconds
        {
            get { return (int)this.CurrentTime.TotalSeconds; }
            set { this.library.CurrentTime = TimeSpan.FromSeconds(value); }
        }

        public bool IsPlaying
        {
            get { return this.library.IsPlaying; }
        }

        public bool IsAdding
        {
            get { return this.isAdding; }
            private set
            {
                if (this.IsAdding != value)
                {
                    this.isAdding = value;
                    this.OnPropertyChanged(vm => vm.IsAdding);
                }
            }
        }

        public string CurrentAddingPath
        {
            get { return this.currentAddingPath; }
            private set
            {
                if (this.CurrentAddingPath != value)
                {
                    this.currentAddingPath = value;
                    this.OnPropertyChanged(vm => vm.CurrentAddingPath);
                }
            }
        }

        public ICommand PlayCommand
        {
            get
            {
                return new RelayCommand
                (
                    param =>
                    {
                        if (this.library.IsPaused && this.SelectedSong == this.library.CurrentSong)
                        {
                            this.library.ContinueSong();
                        }

                        else
                        {
                            this.library.PlaySong(this.SelectedPlaylistSong);
                            this.OnPropertyChanged(vm => vm.TotalSeconds);
                            this.OnPropertyChanged(vm => vm.TotalTime);
                        }

                        this.updateTimer.Start();

                        this.OnPropertyChanged(vm => vm.IsPlaying);
                    }
                );
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                return new RelayCommand
                (
                    param =>
                    {
                        this.library.PauseSong();
                        this.updateTimer.Stop();
                        this.OnPropertyChanged(vm => vm.IsPlaying);
                    }
                );
            }
        }

        public ICommand MuteCommand
        {
            get
            {
                return new RelayCommand
                (
                    param =>
                    {
                        this.Volume = 0;
                    }
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            this.library = new Library();
            this.updateTimer = new Timer(1000);
            this.updateTimer.Elapsed += UpdateTimerElapsed;
        }

        private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.OnPropertyChanged(vm => vm.CurrentSeconds);
            this.OnPropertyChanged(vm => vm.CurrentTime);
        }

        public void AddSelectedSongToPlaylist()
        {
            this.library.AddSongToPlaylist(this.SelectedSong);
            this.OnPropertyChanged(vm => vm.Playlist);
        }

        public void AddSongs(string folderPath)
        {
            EventHandler<SongEventArgs> handler = (sender, e) =>
            {
                this.CurrentAddingPath = e.Song.Path.LocalPath;
            };

            this.library.SongAdded += handler;

            var artistUpdateTimer = new Timer(5000);

            artistUpdateTimer.Elapsed += (sender, e) => this.OnPropertyChanged(vm => vm.Artists);

            this.IsAdding = true;
            artistUpdateTimer.Start();

            this.library
                .AddLocalSongsAsync(folderPath)
                .ContinueWith(task =>
                {
                    this.library.SongAdded -= handler;

                    artistUpdateTimer.Stop();
                    artistUpdateTimer.Dispose();

                    this.OnPropertyChanged(vm => vm.Artists);
                    this.IsAdding = false;
                });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.library.Dispose();
            this.updateTimer.Dispose();
        }
    }
}