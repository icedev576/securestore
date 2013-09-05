using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary; 
using System.Security;
using System.Security.Cryptography; 

/*
 * This is an example script for testing the SecureStore sripts

 Keep all your data safe.
 
 With this script you can easily encrypt and decrypt:
 - A standard string
 - Any file
 - Any stream
 
 You can directly serilalize objects into an encrypt file. Use this for secure user data or anyting else.
 It is using AES-256 algorithm for encrypting, and SHA-256 algorithm for generating private key from password.
 
 Example scene and sript is included and covering all use-cases.
 
 * */

/**
 * Helper class for serialization:
 * */
public sealed class VersionDeserializationBinder : SerializationBinder 
{ 
    public override Type BindToType( string assemblyName, string typeName )
    { 
        if ( !string.IsNullOrEmpty( assemblyName ) && !string.IsNullOrEmpty( typeName ) ) 
        { 
            Type typeToDeserialize = null; 

            assemblyName = Assembly.GetExecutingAssembly().FullName; 

            // The following line of code returns the type. 
            typeToDeserialize = Type.GetType( String.Format( "{0}, {1}", typeName, assemblyName ) ); 

            return typeToDeserialize; 
        } 

        return null; 
    } 
} 

/**
 * Test all use cases:
 * */
public class testSecureStore : MonoBehaviour {

// Declaring test data:
string plainTextString = "This is a message to encrypt.";
string encryptedTextString = "";
string decryptedTextString = "";

string fileToEncrypt = "/testPlainFile.txt";
string encryptedFile = "/testPlainFile.aes";
string decryptedFile = "/testPlainFileDecrypted.txt";

string userDataFile = "/userData.aes";

UserDataTest loadedData = null;	// stores the user data, this is just an example
	
	// Use this for initialization
	void Start () {
		// Set the file location to the Application.persistentDataPath, so we should write here:
		fileToEncrypt = Application.persistentDataPath + fileToEncrypt;
		encryptedFile = Application.persistentDataPath + encryptedFile;
		decryptedFile = Application.persistentDataPath + decryptedFile;
		
		//Create the secure store object:
		SecureStore store = new SecureStore();

		// Testing the string encrypter:
		Debug.Log( "plainText string: " + plainTextString );
		encryptedTextString = store.encryptString( plainTextString );
		Debug.Log( "encrypted string: " + encryptedTextString );
		decryptedTextString = store.decryptString( encryptedTextString );
		Debug.Log( "decrypted string: " + decryptedTextString );
		
		// Testing the file encrypter:
        // Create a file to write to. 
        using( StreamWriter sw = File.CreateText( fileToEncrypt ) ) 
        {
            sw.WriteLine("Hello!");
            sw.WriteLine("This is a plain text file.");
            sw.WriteLine("Encrypt me!");
        }
		Debug.Log( "Text file can be found here: " + fileToEncrypt );
		
		// Now encrypt it:
		store.encryptFile( fileToEncrypt, encryptedFile );
		Debug.Log( "Encryped file can be found here: " + encryptedFile );

		// And also decrypt it:
		store.decryptFile( encryptedFile, decryptedFile );
		Debug.Log( "Decryped file can be found here: " + decryptedFile );
		
		
		// Testing the stream encrypter by serializing an Object:
		UserDataTest data = new UserDataTest( "test-user-7854", "this is test user", 100 );
		
		// Test the saving:
		saveUserData( store, data );
		
		// Test the loading:
		loadedData = loadUserData( store );
		Debug.Log( "Userdata.userGuid: " + loadedData.userGuid );
		Debug.Log( "Userdata.userDescription: " + loadedData.userDescription );
		Debug.Log( "Userdata.userPoints: " + loadedData.userPoints );
	}

	// Just to see something in the Game Window:
    void OnGUI()
    {
		GUI.color = Color.white;
		GUILayout.BeginVertical();
        { 
			GUILayout.TextField( "Plain text:" + plainTextString );
			GUILayout.TextField( "Encrypted text:" + encryptedTextString );
			GUILayout.TextField( "Decrypted text:" + decryptedTextString );
			
			GUILayout.TextField( "" );
			
			GUILayout.TextField( "Plain text file:" + fileToEncrypt );
			GUILayout.TextField( "Encrypted file:" + encryptedFile );
			GUILayout.TextField( "Decrypted file:" + decryptedFile );
			
			// if the data is loaded:
			if( loadedData != null )
			{
				GUILayout.TextField( "" );
				
				GUILayout.TextField( "Userdata.userGuid: " + loadedData.userGuid );
				GUILayout.TextField( "Userdata.userDescription: " + loadedData.userDescription );
				GUILayout.TextField( "Userdata.userPoints: " + loadedData.userPoints );				
			}
			
		}
		GUILayout.EndVertical();
	}
	
	// This method is for testing the encrypted serialization:
	public void saveUserData( SecureStore store, UserDataTest data ){
		string filename = Application.persistentDataPath + userDataFile;
		Debug.Log( "Saving user data to: " + filename );
		
		BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder(); 

        FileStream fsCrypt = new FileStream( filename, FileMode.Create);
		
		// Creating crypto stream via the store:
		CryptoStream cs = store.encryptStream( fsCrypt );
		
		bformatter.Serialize( cs, data );

		cs.Close();
		fsCrypt.Close();						
	} 

	// This method is for testing the decrypted deserialization:
	public UserDataTest loadUserData( SecureStore store  ){
		string filename = Application.persistentDataPath + userDataFile;
		Debug.Log( "Loading user data from: " + filename );		
		
		BinaryFormatter bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder(); 
		
        FileStream fsCrypt = new FileStream( filename, FileMode.Open);

		// Creating crypto stream via the store:
        CryptoStream cs = store.decryptStream( fsCrypt );
		UserDataTest data = (UserDataTest)bformatter.Deserialize(cs);
		
		cs.Close();
		fsCrypt.Close();
						
		return data;
	} 	
	
}