using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Networking;
using System.Collections;

public class SaveasWav : MonoBehaviour
{
    //Größe des Headers
    const int HEADER_SIZE = 44;
    /*
     * Struct um die daten für den Audioclip zu speichern
     *
     */
    struct ClipData
    {

        public int samples;
        public int channels;
        public float[] samplesData;

    }
    

    /*
     * Creating the File and writing empty header
     * header will be filled in later
     * 
     */
    FileStream CreateFile(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }


    public void Save(string filename, AudioClip clip)
    {
        string filepath = Savetofile(filename, clip);
        StartCoroutine(Upload(filepath));
    }


    /*
     * Save an audioclip with the filename
     * 
     */
    public string Savetofile(string filename, AudioClip clip)
    {

        //Filepath konstruieren mit den richtigen änderung
        if (!filename.ToLower().EndsWith(".wav"))
        {
            filename += Time.time+".wav";
        }

        var filepath = Path.Combine(Application.dataPath, filename);
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));
        Debug.Log(filepath);

        //getting the data from the audioclip

        ClipData clipdata = new ClipData();
        clipdata.samples = clip.samples;
        clipdata.channels = clip.channels;
        float[] dataFloat = new float[clip.samples * clip.channels];
        clip.GetData(dataFloat, 0);
        clipdata.samplesData = dataFloat;


        using (var fileStream = CreateFile(filepath))
        {
            MemoryStream memstrm = new MemoryStream();
            ConvertAndWrite(memstrm, clipdata);
            memstrm.WriteTo(fileStream);
            WriteHeader(fileStream, clip);
        }






        return filepath;
    }

    IEnumerator Upload(string filepath)
    {
        WWWForm form = new WWWForm();
        // Open the stream and read it back.
        using (FileStream fs = File.Open(filepath, FileMode.Open))
        {
            byte[] b = new byte[fs.Length];
            fs.Read(b, 0, b.Length);
            form.AddBinaryData("audio", b,"annotation","audio/x-wav");
        }
        

        using (UnityWebRequest www = UnityWebRequest.Post("http://app.stolperwege.hucompute.org/api/audio", form))
        {
            yield return www.SendWebRequest();
            /*
            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }*/
        }
    }


    /*
     * Getting the data from the audioclip and converting 
     * it so it can be written as bytes into the MemoryStream
     */

    void ConvertAndWrite(MemoryStream memStream, ClipData clipData)
    {
        float[] samples = new float[clipData.samples * clipData.channels];

        samples = clipData.samplesData;

        Int16[] intData = new Int16[samples.Length];

        Byte[] bytesData = new Byte[samples.Length * 2];

        const float rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            //Debug.Log (samples [i]);
        }
        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        memStream.Write(bytesData, 0, bytesData.Length);
    }



    /*
     * writing the standard wav header
     * 
     * 	'RIFF'
     * 	<Dateigröße> − 8
     * 	'WAVE'
     * 	'fmt ' Header-Signatur (folgendes Leerzeichen beachten)
     * 	<fmt length>	Länge des restlichen fmt-Headers (16 Bytes)
     * 	<format tag>	Datenformat der Abtastwerte (siehe separate Tabelle weiter unten)
     * 	<channels>	Anzahl der Kanäle: 1 = mono, 2 = stereo; 
     * 	<sample rate>	Samples pro Sekunde je Kanal
     * 	<bytes/second>	Abtastrate · Frame-Größe
     * 	<block align>	Frame-Größe = <Anzahl der Kanäle> · ((<Bits/Sample (eines Kanals)> + 7) / 8) 
     * 	<bits/sample>	Anzahl der Datenbits pro Samplewert je Kanal
     * 	'data'	Header-Signatur
     * 	<length>	Länge des Datenblocks, max. <Dateigröße> − 44
     */
    void WriteHeader(FileStream fileStream, AudioClip clip)
    {

        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);
        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * 2);
        fileStream.Write(subChunk2, 0, 4);

        //		fileStream.Close();
    }
}