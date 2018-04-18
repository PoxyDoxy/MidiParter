using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidiParter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(openFileDialog1.FileNames);
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] filenamelist = (string[])e.Argument;

            this.Invoke(new MethodInvoker(delegate () { button1.Visible = false; }));
            int totalfilecount = filenamelist.Count();
            int processedfilecount = 0;
            int failedfiles = 0;
            this.Invoke(new MethodInvoker(delegate () { label2.Text = processedfilecount.ToString() + "/" + totalfilecount.ToString(); }));

            foreach (string filename in filenamelist)
            {
                string input_folder = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
                string input_filename = Path.GetFileName(filename);

                if (!File.Exists(input_folder + input_filename))
                {
                    MessageBox.Show("Could not load MIDI File (" + input_folder + input_filename + "), does not exist!");
                    return;
                }

                var inputmidi = MidiFile.Read(input_folder + input_filename);

                // Track Amount
                int track_count = inputmidi.GetTrackChunks().Count();

                if (track_count <= 0)
                {
                    MessageBox.Show("No tracks were found in your MIDI File(" + input_folder + input_filename + ")!");
                    return;
                }

                // Track Names NAudio.Midi
                try
                {
                    NAudio.Midi.MidiFile inputmidi2 = new NAudio.Midi.MidiFile(input_folder + input_filename);

                    for (int i = 0; i < inputmidi2.Tracks; i++)
                    {
                        string output_filename2 = Regex.Replace(input_filename, ".mid", "") + " - track" + i + ".mid";

                        //MessageBox.Show(i.ToString());
                        //MessageBox.Show(output_filename2);
                        // Run DryWetMidi Code to Strip Notes and Save

                        var clean_inputmidi = MidiFile.Read(input_folder + input_filename);
                        foreach (var trackChunk in clean_inputmidi.GetTrackChunks())
                        {
                            //MessageBox.Show("New Track Chunk");

                            /*foreach (var ch in inputmidi.GetChords()) {
                                MessageBox.Show(ch.Channel.ToString());
                            }*/

                            using (var notesManager = trackChunk.ManageNotes())
                            {
                                foreach (var nt in notesManager.Notes)
                                {
                                    //MessageBox.Show(nt.NoteName.ToString());
                                    //MessageBox.Show(nt.NoteName.ToString() + " | " + nt.Channel.ToString() + " | " + nt.Octave.ToString());
                                    if (nt.Channel != i)
                                    {
                                        //MessageBox.Show(notesManager.Notes.Count().ToString());
                                        //MessageBox.Show(notesManager.Notes.ToString());

                                        // Set Velocity to Zero
                                        //notesManager.Notes.Remove(nt);
                                        nt.Velocity = (SevenBitNumber)0;
                                        //notesManager.Notes.Remove(nt);

                                        //notesManager.Notes.RemoveAll(n => n.NoteName == NoteName.C);
                                        //notesManager.Notes.RemoveAll(n => n.NoteName == NoteName.D);
                                    }
                                }
                            }
                        }

                        string temp_folder_slice = "";

                        if (checkBox2.Checked)
                        {
                            // Save Tracks to Folder
                            temp_folder_slice = Regex.Replace(input_filename, ".mid", "") + Path.DirectorySeparatorChar;
                            if (!Directory.Exists(input_folder + temp_folder_slice))
                            {
                                Directory.CreateDirectory(input_folder + temp_folder_slice);
                            }
                        }

                        if (checkBox1.Checked)
                        {
                            // Remove Existing Files
                            if (File.Exists(input_folder + temp_folder_slice + output_filename2))
                            {
                                File.Delete(input_folder + temp_folder_slice + output_filename2);
                            }
                        }

                        if (!File.Exists(input_folder + temp_folder_slice + output_filename2))
                        {
                            //var trackChunk = new TrackChunk(new TextEvent("Bleepity Bloop"), new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)45), new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0) { DeltaTime = 400, } );
                            //clean_inputmidi.Chunks.Add(trackChunk);
                            clean_inputmidi.Write(input_folder + temp_folder_slice + output_filename2);
                        }
                    }
                }
                catch (Exception ee)
                {
                    failedfiles++;
                    //MessageBox.Show("Could not load " + input_filename + ".");
                }
                // End of NAudio.Midi
                processedfilecount++;
                this.Invoke(new MethodInvoker(delegate () { label2.Text = processedfilecount.ToString() + "/" + totalfilecount.ToString(); }));
            }
            this.Invoke(new MethodInvoker(delegate () { button1.Visible = true; }));
            if (failedfiles >= 1)
            {
                MessageBox.Show("Done (could not process " + failedfiles.ToString() + " file)");
            }
            else {
                MessageBox.Show("Done (" + processedfilecount + " files processed)!");
            }
            
        }
    }
}
