﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KartRider;
using KartRider.Record;
using KartRider.Xml;

#nullable enable
namespace RhoLoader.PreviewWindow
{
    public partial class KSVPreview : Form
    {
        private readonly BinaryXmlTag? LocaleData = null;
        public KSVPreview(byte[] Data, BinaryXmlTag LocaleData)
        {
            InitializeComponent();
            this.LocaleData = LocaleData;
            ReadKSV(Data);
        }

        public void ReadKSV(byte[] data)
        {
            KSVInfo ksvinfo = KartRecord.ReadKSVFromBytes(data);
            this.contestName.Text = ksvinfo.RecordTitle;
            infoBox.Items.Add(new ListViewItem(new string[] { "ksv_Contesttype".GetStringBag(),
                ksvinfo.ContestType switch
                {
                    ContestType.InvalidGameType => "Invalid",
                    ContestType.SpeedIndividual => "ksv_SpeedIndividual".GetStringBag(),
                    ContestType.SpeedTeam => "ksv_SpeedTeam".GetStringBag(),
                    ContestType.ItemIndividual => "ksv_ItemIndividual".GetStringBag(),
                    ContestType.ItemTeam => "ksv_ItemTeam".GetStringBag(),
                    _ => ksvinfo.ContestType.ToString()
                }}));
            infoBox.Items.Add(new ListViewItem(new string[] { "ksv_RecordingDate".GetStringBag(), $"{ksvinfo.RecordingDate:yyyy/MM/dd HH:mm:ss}"}));
            infoBox.Items.Add(new ListViewItem(new string[] { "ksv_Region".GetStringBag(), ksvinfo.RegionCode.ToString()}));
            infoBox.Items.Add(new ListViewItem(new string[] { "ksv_TrackName".GetStringBag(), ReadTrackName(ksvinfo.TrackName) }));
            foreach (PlayerInfo pi in ksvinfo.Players)
            {
                ListViewItem lvi = new ListViewItem(pi.PlayerName);
                int team = pi.Equipment.Equ5 >> 8;
                if (team == 1)
                    lvi.BackColor = Color.OrangeRed;
                else if(team == 2)
                    lvi.BackColor = Color.SkyBlue;
                players.Items.Add(lvi);
            }
        }

        private void Players_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                if (e.Item != null)
                {
                    e.Item.Selected = false;
                    e.Item.Focused = false;
                }
            }
        }

        private void Players_MouseHover(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void KSVPreview_Load(object sender, EventArgs e)
        {

        }

        private string ReadTrackName(string track_name)
        {
            if (LocaleData is null)
                return track_name;
            BinaryXmlTag? found_tag = LocaleData.SubTags.Find(x=>x.GetAttribute("id") == track_name);
            if(found_tag is not null)
                return found_tag.GetAttribute("name");
            else
                return track_name;
        }
    }
}