using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SimpleJSON;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.Common.Utilities;

namespace AIUniversalTranslate
{
   /// <summary>
   /// Universal AI translation endpoint for XUnity.AutoTranslator.
   /// Supports any OpenAI-compatible API including OpenAI, DeepSeek, Tongyi Qianwen, Ollama, etc.
   /// </summary>
   public class AIUniversalTranslateEndpoint : HttpEndpoint
   {
      private static readonly Dictionary<string, string> LanguageNames = new Dictionary<string, string>
      {
         { "ja", "日语" },
         { "jp", "日语" },
         { "zh", "中文" },
         { "zh-Hans", "简体中文" },
         { "zh-CN", "简体中文" },
         { "zh-Hant", "繁体中文" },
         { "zh-TW", "繁体中文" },
         { "en", "英语" },
         { "ko", "韩语" },
         { "kor", "韩语" },
         { "fr", "法语" },
         { "fra", "法语" },
         { "de", "德语" },
         { "ru", "俄语" },
         { "es", "西班牙语" },
         { "spa", "西班牙语" },
         { "it", "意大利语" },
         { "pt", "葡萄牙语" },
         { "th", "泰语" },
         { "vi", "越南语" },
         { "vie", "越南语" },
         { "ar", "阿拉伯语" },
         { "ara", "阿拉伯语" },
         { "pl", "波兰语" },
         { "cs", "捷克语" },
         { "hu", "匈牙利语" },
         { "nl", "荷兰语" },
         { "sv", "瑞典语" },
         { "swe", "瑞典语" },
         { "tr", "土耳其语" },
         { "id", "印尼语" }
      };

      private string _apiUrl;
      private string _apiKey;
      private string _model;
      private string _systemPrompt;
      private float _temperature;
      private int _maxTokens;
      private float _delay;
      private bool _disableSpamChecks;
      private float _lastRequestTimestamp;

      public override string Id => "AIUniversalTranslate";

      public override string FriendlyName => "AI Universal Translator";

      public override int MaxConcurrency => 1;

      public override int MaxTranslationsPerRequest => 1;

      public override void Initialize( IInitializationContext context )
      {
         _apiUrl = context.GetOrCreateSetting( "AIUniversal", "ApiUrl", "" );
         _apiKey = context.GetOrCreateSetting( "AIUniversal", "ApiKey", "" );
         _model = context.GetOrCreateSetting( "AIUniversal", "Model", "gpt-3.5-turbo" );
         _systemPrompt = context.GetOrCreateSetting( "AIUniversal", "SystemPrompt",
            "你是一个专业的游戏翻译助手。请将用户提供的文本翻译成目标语言，保持原文的语气和风格。只输出翻译结果，不要添加解释，不要输出原文。" );
         _temperature = context.GetOrCreateSetting( "AIUniversal", "Temperature", 0.3f );
         _maxTokens = context.GetOrCreateSetting( "AIUniversal", "MaxTokens", 2048 );
         _delay = context.GetOrCreateSetting( "AIUniversal", "DelaySeconds", 1.0f );
         _disableSpamChecks = context.GetOrCreateSetting( "AIUniversal", "DisableSpamChecks", false );

         if( string.IsNullOrEmpty( _apiUrl ) ) throw new EndpointInitializationException( "AIUniversalTranslate 端点需要配置 ApiUrl。" );
         if( string.IsNullOrEmpty( _apiKey ) ) throw new EndpointInitializationException( "AIUniversalTranslate 端点需要配置 ApiKey。" );

         var uri = new Uri( _apiUrl );
         context.DisableCertificateChecksFor( uri.Host );

         if( _disableSpamChecks ) context.DisableSpamChecks();
      }

      public override IEnumerator OnBeforeTranslate( IHttpTranslationContext context )
      {
         var realtimeSinceStartup = TimeHelper.realtimeSinceStartup;

         var timeSinceLast = realtimeSinceStartup - _lastRequestTimestamp;
         if( timeSinceLast < _delay )
         {
            var delay = _delay - timeSinceLast;

            var instruction = CoroutineHelper.CreateWaitForSecondsRealtime( delay );
            if( instruction != null )
            {
               yield return instruction;
            }
            else
            {
               float start = realtimeSinceStartup;
               var end = start + delay;
               while( TimeHelper.realtimeSinceStartup < end )
               {
                  yield return null;
               }
            }
         }

         _lastRequestTimestamp = TimeHelper.realtimeSinceStartup;
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         string sourceLang = GetLanguageName( context.SourceLanguage );
         string targetLang = GetLanguageName( context.DestinationLanguage );
         string text = context.UntranslatedText;

         // Build user prompt
         string userPrompt = string.Format(
            "请将以下文本从{0}翻译到{1}：\n\n{2}",
            sourceLang, targetLang, text );

         // Build JSON request body
         var json = new JSONObject();
         json["model"] = _model;

         var messages = new JSONArray();
         var systemMsg = new JSONObject();
         systemMsg["role"] = "system";
         systemMsg["content"] = _systemPrompt;
         messages.Add( systemMsg );

         var userMsg = new JSONObject();
         userMsg["role"] = "user";
         userMsg["content"] = userPrompt;
         messages.Add( userMsg );

         json["messages"] = messages;
         json["temperature"] = _temperature;
         json["max_tokens"] = _maxTokens;

         string data = json.ToString();

         var request = new XUnityWebRequest( "POST", _apiUrl, data );
         request.Headers[ HttpRequestHeader.ContentType ] = "application/json";
         request.Headers[ "Authorization" ] = "Bearer " + _apiKey;

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;

         if( string.IsNullOrEmpty( data ) )
         {
            context.Fail( "AIUniversalTranslate 端点返回了空数据。" );
            return;
         }

         var obj = JSON.Parse( data );
         if( obj == null )
         {
            context.Fail( "无法解析 AIUniversalTranslate 端点的 JSON 响应。" );
            return;
         }

         // Check for API errors using string guard first, then JSON
         if( data.Contains( "\"error\"" ) )
         {
            var errorNode = obj.AsObject["error"];
            string errorMsg = "Unknown error";
            try
            {
               if( errorNode != null && errorNode.IsObject && errorNode.Count > 0 )
               {
                  var msgNode = errorNode.AsObject["message"];
                  if( msgNode != null && msgNode.IsString )
                  {
                     errorMsg = msgNode.ToString().Trim( '"' );
                  }
               }
            }
            catch { }
            context.Fail( "AIUniversalTranslate API 错误: " + errorMsg );
            return;
         }

         // Extract translation from choices
         var choicesNode = obj.AsObject["choices"];
         if( !choicesNode.IsArray || choicesNode.AsArray.Count == 0 )
         {
            // Fallback for some non-standard APIs (text completion format)
            var textNode = obj.AsObject["text"];
            if( textNode != null && textNode.IsString )
            {
               context.Complete( textNode.ToString().Trim( '"' ) );
               return;
            }
            context.Fail( "AIUniversalTranslate 响应中未找到翻译结果。" );
            return;
         }

         var choices = choicesNode.AsArray;

         // Try standard chat completion format: choices[0].message.content
         var messageNode = choices[0].AsObject["message"];
         if( messageNode != null && messageNode.IsObject )
         {
            var contentNode = messageNode.AsObject["content"];
            if( contentNode != null && contentNode.IsString )
            {
               context.Complete( contentNode.ToString().Trim( '"' ) );
               return;
            }
         }

         // Fallback: stream delta format (should not appear, but just in case)
         var deltaNode = choices[0].AsObject["delta"];
         if( deltaNode != null && deltaNode.IsObject )
         {
            var contentNode = deltaNode.AsObject["content"];
            if( contentNode != null && contentNode.IsString )
            {
               context.Complete( contentNode.ToString().Trim( '"' ) );
               return;
            }
         }

         context.Fail( "无法从 AIUniversalTranslate 端点响应中提取翻译。" );
      }

      private string GetLanguageName( string langCode )
      {
         if( LanguageNames.TryGetValue( langCode, out var name ) )
         {
            return name;
         }
         return langCode;
      }
   }
}
