﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DecklistManagement
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DecklistDownloader.DownloadPioneerDecklists();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DecklistDownloader.DownloadModernDecklists();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DecklistDownloader.CombinePioneerDecklists();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DecklistDownloader.CombineModernDecklists();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DecklistDownloader.FindPioneerModernDecklistDifferences();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DecklistDownloader.CalculatePioneerCompletionPercentage();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DecklistDownloader.CalculateModernCompletionPercentage();
        }
    }
}
