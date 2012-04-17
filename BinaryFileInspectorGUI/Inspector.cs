using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using BFSchema;

namespace BinaryFileInspectorGUI
{
    public partial class Inspector : Form
    {
        BinaryFileSchema schema;
        BfsBinaryReader reader;

        public Inspector()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenSchema(@"C:\Users\Anders\Desktop\Binary File Schema\photoshop.fsc");
            OpenFile(@"C:\Users\Anders\Desktop\Binary File Schema\128x512x4.psd");
        }

        public void OpenSchema(string filename)
        {
            BfsErrorHandler errorHandler = new BfsErrorHandler(listView1);
            toolStripStatusLabel1.Text = "Opening schema: " + filename;
            schema = new BinaryFileSchema(filename, errorHandler);
        }

        public void OpenFile(string filename)
        {
            BinaryReader b_reader = new BinaryReader(new FileStream(filename, FileMode.Open));
            
            BfsBinaryReader.Endianness endianness;
            if(schema.ByteOrder.ByteOrder == BfsByteOrderEnum.BigEndian)
                endianness = BfsBinaryReader.Endianness.BigEndian;
            else
                endianness = BfsBinaryReader.Endianness.LittleEndian;

            reader = new BfsBinaryReader(b_reader, endianness);
            TreeNode rootNode = new TreeNode(schema.FormatBlock.Name);
            treeView1.Nodes.Add(rootNode);
            ReadDataBlock(schema.FormatBlock, rootNode);
            rootNode.ExpandAll();
        }

        private void ReadDataBlock(IBfsDataBlock block, TreeNode parent)
        {
            if (block is BfsStruct)
                ReadStruct(block as BfsStruct, parent);
            else if (block is BfsEnum)
                ReadEnum(block as BfsEnum, parent);
            else if (block is BfsBitfield)
                ReadBitField(block as BfsBitfield, parent);
        }

        private void AddFileLine(string data, IBfsType type, TreeNode parent)
        {
            TreeNode node = new TreeNode(data);
            node.Tag = type;
            parent.Nodes.Add(node);
        }

        private void ReadStruct(BfsStruct bfsstruct, TreeNode parent)
        {
            foreach(BfsStructField field in bfsstruct.StructFieldList)
            {
                TreeNode nodeParent = parent;
                if (field.FieldType.ArrayExtension is BfsKnownArray)
                {
                    BfsKnownArray knownArray = field.FieldType.ArrayExtension as BfsKnownArray;
                    TreeNode arrayParent = new TreeNode(field.Name + " []");
                    parent.Nodes.Add(arrayParent);
                    //TODO: Evaluate expression and use that number for the loop
                    //for(int i = 0; i<knownArray.
                }

                
                

            }
        }

        private void ReadStructField(BfsStructField field, TreeNode parent)
        {
            if (field.FieldType is BfsFunctionType)
            {
                BfsFunctionType functionType = field.FieldType as BfsFunctionType;
                if (functionType.FunctionName == "ascii")
                    AddFileLine(field.Name + " " + reader.ReadASCIIString(functionType.FunctionArgument), field.FieldType, parent);
            }

            if (field.FieldType is BfsPrimitiveType)
            {
                BfsPrimitiveType primitiveType = field.FieldType as BfsPrimitiveType;
                switch (primitiveType.PrimitiveType)
                {
                    case BfsPrimitiveTypeEnum.Bool:
                        AddFileLine(field.Name + " " + reader.ReadBool(), field.FieldType, parent);
                        break;
                    case BfsPrimitiveTypeEnum.Sbyte:
                        AddFileLine(field.Name + " " + reader.ReadSbyte(), field.FieldType, parent);
                        break;
                    case BfsPrimitiveTypeEnum.Ubyte:
                        AddFileLine(field.Name + " " + reader.ReadUbyte(), field.FieldType, parent);
                        break;
                    case BfsPrimitiveTypeEnum.Int:
                        AddFileLine(field.Name + " " + reader.ReadInt(), field.FieldType, parent);
                        break;
                    case BfsPrimitiveTypeEnum.Uint:
                        AddFileLine(field.Name + " " + reader.ReadUint(), field.FieldType, parent);
                        break;
                    case BfsPrimitiveTypeEnum.Short:
                        AddFileLine(field.Name + " " + reader.ReadShort(), field.FieldType, parent);
                        break;
                    case BfsPrimitiveTypeEnum.Ushort:
                        AddFileLine(field.Name + " " + reader.ReadUshort(), field.FieldType, parent);
                        break;
                    case BfsPrimitiveTypeEnum.Long:
                        AddFileLine(field.Name + " " + reader.ReadLong(), field.FieldType, parent);
                        break;
                    case BfsPrimitiveTypeEnum.Ulong:
                        AddFileLine(field.Name + " " + reader.ReadUlong(), field.FieldType, parent);
                        break;
                }
            }

            if (field.FieldType is BfsNamedType)
            {
                BfsNamedType namedType = field.FieldType as BfsNamedType;
                if (namedType.DataBlock is BfsStruct)
                {
                    BfsStruct subStruct = namedType.DataBlock as BfsStruct;
                    TreeNode newNode = new TreeNode(subStruct.Name);
                    parent.Nodes.Add(newNode);
                    ReadDataBlock(namedType.DataBlock, newNode);
                }

            }
        }
        

        private void ReadEnum(BfsEnum bfsenum, TreeNode parent)
        {
        
        }

        private void ReadBitField(BfsBitfield bfsbitfield, TreeNode parent)
        {

        }


    }
}
