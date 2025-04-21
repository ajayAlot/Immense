// using System;
// using System.Text.Json;
// using System.Text.Json.Serialization;

// namespace processJobAndSmsApi
// {
// public class StringToIntArrayConverter : JsonConverter<int[]>
// {
//     public override int[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//     {
//         if (reader.TokenType == JsonTokenType.String)
//         {
//             // Handle case where the input is a single string (could be comma-separated)
//             var stringValue = reader.GetString();
//             if (!string.IsNullOrEmpty(stringValue))
//             {
//                 // Explicitly define the split as using char[] instead of string[]
//                 var stringArray = stringValue.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
//                 return Array.ConvertAll(stringArray, int.Parse);
//             }
//         }
//         else if (reader.TokenType == JsonTokenType.Number)
//         {
//             // Handle case where it's a single numeric value
//             return new[] { reader.GetInt32() };
//         }
//         else if (reader.TokenType == JsonTokenType.StartArray)
//         {
//             // Handle case where it's an array of numbers or strings
//             var elements = new System.Collections.Generic.List<int>();
//             while (reader.Read())
//             {
//                 if (reader.TokenType == JsonTokenType.EndArray)
//                 {
//                     break;
//                 }

//                 if (reader.TokenType == JsonTokenType.String)
//                 {
//                     elements.Add(int.Parse(reader.GetString()));
//                 }
//                 else if (reader.TokenType == JsonTokenType.Number)
//                 {
//                     elements.Add(reader.GetInt32());
//                 }
//             }
//             return elements.ToArray();
//         }

//         throw new JsonException("Invalid JSON format for SmsLimit");
//     }

//     public override void Write(Utf8JsonWriter writer, int[] value, JsonSerializerOptions options)
//     {
//         // Writing the array of integers back to JSON format
//         writer.WriteStartArray();
//         foreach (var number in value)
//         {
//             writer.WriteNumberValue(number);
//         }
//         writer.WriteEndArray();
//         }
//     }
// }