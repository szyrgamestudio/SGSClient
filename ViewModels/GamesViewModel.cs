using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SGSClient.ViewModels
{
    public class GamesViewModel : ObservableRecipient
    {
        public string _gameId;
        private string _gameSymbol;
        private string _gameName;
        private string _gameDeveloper;
        private string _gameTitle;
        private Uri _imageSource;
        private string _gameVersion;
        private string _gamePayloadName;
        private string _gameExeName;
        private string _gameZipLink;
        private string _gameVersionLink;
        private string _gameDescription;
        private string _hardwareRequirements;
        private string _otherInformations;
        private string _logoPath;
        private string _gameType;
        private string _draftP;

        public string GameId
        {
            get => _gameId;
            set => SetProperty(ref _gameId, value);
        }
        public string GameSymbol
        {
            get => _gameSymbol;
            set => SetProperty(ref _gameSymbol, value);
        }
        public string GameName
        {
            get => _gameName;
            set => SetProperty(ref _gameName, value);
        }

        public string GameDeveloper
        {
            get => _gameDeveloper;
            set => SetProperty(ref _gameDeveloper, value);
        }

        public string GameTitle
        {
            get => _gameTitle;
            set => SetProperty(ref _gameTitle, value);
        }

        public Uri ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public string GameVersion
        {
            get => _gameVersion;
            set => SetProperty(ref _gameVersion, value);
        }

        public string GamePayloadName
        {
            get => _gamePayloadName;
            set => SetProperty(ref _gamePayloadName, value);
        }

        public string GameExeName
        {
            get => _gameExeName;
            set => SetProperty(ref _gameExeName, value);
        }

        public string GameZipLink
        {
            get => _gameZipLink;
            set => SetProperty(ref _gameZipLink, value);
        }

        public string GameVersionLink
        {
            get => _gameVersionLink;
            set => SetProperty(ref _gameVersionLink, value);
        }

        public string GameDescription
        {
            get => _gameDescription;
            set => SetProperty(ref _gameDescription, value);
        }

        public string HardwareRequirements
        {
            get => _hardwareRequirements;
            set => SetProperty(ref _hardwareRequirements, value);
        }

        public string OtherInformations
        {
            get => _otherInformations;
            set => SetProperty(ref _otherInformations, value);
        }

        public string LogoPath
        {
            get => _logoPath;
            set => SetProperty(ref _logoPath, value);
        }

        public string GameType
        {
            get => _gameType;
            set => SetProperty(ref _gameType, value);
        }
        public string DraftP
        {
            get => _draftP;
            set => SetProperty(ref _draftP, value);
        }

        // Aktualizacja konstruktora
        public GamesViewModel(string gameId, string gameSymbol, string gameTitle, string gamePayloadName, string gameExeName,
            string gameZipLink, string gameVersionLink, string gameDescription, string hardwareRequirements, string otherInformations, string gameDeveloper, string logoPath, string gameType, string draftP)
        {
            GameId = gameId;
            GameSymbol = gameSymbol;
            GameTitle = gameTitle;
            GamePayloadName = gamePayloadName;
            GameExeName = gameExeName;
            GameZipLink = gameZipLink;
            GameVersionLink = gameVersionLink;
            GameDescription = gameDescription;
            HardwareRequirements = hardwareRequirements;
            OtherInformations = otherInformations;
            GameDeveloper = gameDeveloper;
            LogoPath = logoPath;
            GameType = gameType;
            DraftP = draftP;
        }
    }
}
