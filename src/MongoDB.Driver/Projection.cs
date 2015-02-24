﻿/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Translators;

namespace MongoDB.Driver
{
    /// <summary>
    /// A rendered projection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class RenderedProjection<TDocument>
    {
        private readonly BsonDocument _projection;
        private readonly IBsonSerializer<TDocument> _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderedProjection{TDocument}" /> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="serializer">The serializer.</param>
        public RenderedProjection(BsonDocument document, IBsonSerializer<TDocument> serializer)
        {
            _projection = document;
            _serializer = Ensure.IsNotNull(serializer, "serializer");
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public BsonDocument Document
        {
            get { return _projection; }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }
    }

    /// <summary>
    /// Base class for projections without a result type.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public abstract class Projection<TDocument>
    {
        /// <summary>
        /// Turns the projection into a typed projection.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <returns>A typed projection.</returns>
        public virtual Projection<TDocument, TResult> As<TResult>(IBsonSerializer<TResult> resultSerializer = null)
        {
            return new TypedProjectionAdapter<TDocument, TResult>(this, resultSerializer);
        }

        /// <summary>
        /// Renders the projection to a <see cref="RenderedProjection{TResult}"/>.
        /// </summary>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>A <see cref="BsonDocument"/>.</returns>
        public abstract BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry);

        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonDocument"/> to <see cref="Projection{TDocument}"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Projection<TDocument>(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new BsonDocumentProjection<TDocument>(document);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String" /> to <see cref="Projection{TDocument, TResult}" />.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Projection<TDocument>(string json)
        {
            if (json == null)
            {
                return null;
            }

            return new JsonStringProjection<TDocument>(json);
        }
    }

    /// <summary>
    /// Base class for projections.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class Projection<TDocument, TResult>
    {
        /// <summary>
        /// Renders the projection to a <see cref="RenderedProjection{TResult}"/>.
        /// </summary>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>A <see cref="RenderedProjection{TResult}"/>.</returns>
        public abstract RenderedProjection<TResult> Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry);

        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonDocument"/> to <see cref="Projection{TDocument, TResult}"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Projection<TDocument, TResult>(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new BsonDocumentProjection<TDocument, TResult>(document);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String" /> to <see cref="Projection{TDocument, TResult}" />.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Projection<TDocument, TResult>(string json)
        {
            if (json == null)
            {
                return null;
            }

            return new JsonStringProjection<TDocument, TResult>(json);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Projection{TDocument}"/> to <see cref="Projection{TDocument, TResult}"/>.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Projection<TDocument, TResult>(Projection<TDocument> projection)
        {
            return new TypedProjectionAdapter<TDocument, TResult>(projection);
        }
    }

    /// <summary>
    /// A <see cref="BsonDocument" /> based projection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class BsonDocumentProjection<TDocument> : Projection<TDocument>
    {
        private readonly BsonDocument _document;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentProjection{TDocument}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public BsonDocumentProjection(BsonDocument document)
        {
            _document = Ensure.IsNotNull(document, "document");
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public BsonDocument Document
        {
            get { return _document; }
        }

        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return _document;
        }
    }

    /// <summary>
    /// A <see cref="BsonDocument" /> based projection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class BsonDocumentProjection<TDocument, TResult> : Projection<TDocument, TResult>
    {
        private readonly BsonDocument _document;
        private readonly IBsonSerializer<TResult> _resultSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentProjection{TDocument, TResult}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        public BsonDocumentProjection(BsonDocument document, IBsonSerializer<TResult> resultSerializer = null)
        {
            _document = Ensure.IsNotNull(document, "document");
            _resultSerializer = resultSerializer;
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public BsonDocument Document
        {
            get { return _document; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        /// <inheritdoc />
        public override RenderedProjection<TResult> Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedProjection<TResult>(
                _document,
                _resultSerializer ?? (documentSerializer as IBsonSerializer<TResult>) ?? serializerRegistry.GetSerializer<TResult>());
        }
    }

    /// <summary>
    /// A find <see cref="Expression" /> based projection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class FindExpressionProjection<TDocument, TResult> : Projection<TDocument, TResult>
    {
        private readonly Expression<Func<TDocument, TResult>> _expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindExpressionProjection{TDocument, TResult}" /> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public FindExpressionProjection(Expression<Func<TDocument, TResult>> expression)
        {
            _expression = Ensure.IsNotNull(expression, "expression");
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Expression<Func<TDocument, TResult>> Expression
        {
            get { return _expression; }
        }

        /// <inheritdoc />
        public override RenderedProjection<TResult> Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return FindProjectionTranslator.Translate<TDocument, TResult>(_expression, documentSerializer);
        }
    }

    /// <summary>
    /// A <see cref="String" /> based projection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class JsonStringProjection<TDocument> : Projection<TDocument>
    {
        private readonly string _json;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonStringProjection{TDocument}"/> class.
        /// </summary>
        /// <param name="json">The json.</param>
        public JsonStringProjection(string json)
        {
            _json = Ensure.IsNotNull(json, "json");
        }

        /// <summary>
        /// Gets the json.
        /// </summary>
        public string Json
        {
            get { return _json; }
        }

        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return BsonDocument.Parse(_json);
        }
    }

    /// <summary>
    /// A <see cref="String" /> based projection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class JsonStringProjection<TDocument, TResult> : Projection<TDocument, TResult>
    {
        private readonly string _json;
        private readonly IBsonSerializer<TResult> _resultSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentSort{TDocument}" /> class.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        public JsonStringProjection(string json, IBsonSerializer<TResult> resultSerializer = null)
        {
            _json = Ensure.IsNotNull(json, "json");
            _resultSerializer = resultSerializer;
        }

        /// <summary>
        /// Gets the json.
        /// </summary>
        public string Json
        {
            get { return _json; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        /// <inheritdoc />
        public override RenderedProjection<TResult> Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedProjection<TResult>(
                BsonDocument.Parse(_json),
                _resultSerializer ?? (documentSerializer as IBsonSerializer<TResult>) ?? serializerRegistry.GetSerializer<TResult>());
        }
    }

    /// <summary>
    /// A <see cref="Object"/> based projection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class ObjectProjection<TDocument> : Projection<TDocument>
    {
        private readonly object _obj;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectProjection{TDocument}"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        public ObjectProjection(object obj)
        {
            _obj = Ensure.IsNotNull(obj, "obj");
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        public object Object
        {
            get { return _obj; }
        }

        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var serializer = serializerRegistry.GetSerializer(_obj.GetType());
            return new BsonDocumentWrapper(_obj, serializer);
        }
    }

    /// <summary>
    /// A <see cref="Object"/> based projection.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class ObjectProjection<TDocument, TResult> : Projection<TDocument, TResult>
    {
        private readonly object _obj;
        private readonly IBsonSerializer<TResult> _resultSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectProjection{TDocument, TResult}" /> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        public ObjectProjection(object obj, IBsonSerializer<TResult> resultSerializer = null)
        {
            _obj = Ensure.IsNotNull(obj, "obj");
            _resultSerializer = resultSerializer;
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        public object Object
        {
            get { return _obj; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        /// <inheritdoc />
        public override RenderedProjection<TResult> Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var serializer = serializerRegistry.GetSerializer(_obj.GetType());
            return new RenderedProjection<TResult>(
                new BsonDocumentWrapper(_obj, serializer),
                _resultSerializer ?? (documentSerializer as IBsonSerializer<TResult>) ?? serializerRegistry.GetSerializer<TResult>());
        }
    }

    internal sealed class TypedProjectionAdapter<TDocument, TResult> : Projection<TDocument, TResult>
    {
        private readonly Projection<TDocument> _projection;
        private readonly IBsonSerializer<TResult> _resultSerializer;

        public TypedProjectionAdapter(Projection<TDocument> projection, IBsonSerializer<TResult> resultSerializer = null)
        {
            _projection = Ensure.IsNotNull(projection, "projection");
            _resultSerializer = resultSerializer;
        }

        public Projection<TDocument> Projection
        {
            get { return _projection; }
        }

        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        public override RenderedProjection<TResult> Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedProjection = _projection.Render(documentSerializer, serializerRegistry);
            return new RenderedProjection<TResult>(
                renderedProjection,
                _resultSerializer ?? (documentSerializer as IBsonSerializer<TResult>) ?? serializerRegistry.GetSerializer<TResult>());
        }
    }

    internal sealed class TypeProjection<TDocument, TResult> : Projection<TDocument, TResult>
    {
        private readonly IBsonSerializer<TResult> _resultSerializer;

        public TypeProjection(IBsonSerializer<TResult> resultSerializer = null)
        {
            _resultSerializer = resultSerializer;
        }

        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        public override RenderedProjection<TResult> Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedProjection<TResult>(
                null,
                _resultSerializer ?? (documentSerializer as IBsonSerializer<TResult>) ?? serializerRegistry.GetSerializer<TResult>());
        }
    }
}