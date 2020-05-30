import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { actionCreators } from '../store/WikiWalks';
//import { getEnglishDate } from '../common/functions';
//import Head from './Helmet';

class Category extends Component {

    constructor(props) {
        super(props);

        const category = this.props.match.params.category.split("_").join(" ");
        this.state = {
            pages: [],
            category,
        }
    }

    componentDidMount() {
        const getData = async () => {
            const url = `api/WikiWalks/getWordsForCategory?category=${this.state.category}`;
            const response = await fetch(url);
            const pages = await response.json();
            this.setState({ pages });
        }
        getData();
    }

    render() {
        const { wordId } = this.props.pages;
        const { pages, category } = this.state;
        const description = `This is a list of the keywords about ${category ? category : "..."}. Please check the words below to know about ${category ? category : "..."}!`;
        const arrDesc = description.split(". ");
        const lineChangeDesc = arrDesc.map((d, i) => <span key={i}>{d}{i < arrDesc.length - 1 && ". "}<br /></span>);
        return (
            <div>
                {/*<Head
                    title={title}
                    desc={description}
                />*/}
                <div className="breadcrumbs" itemScope itemType="https://schema.org/BreadcrumbList" style={{ textAlign: "left" }}>
                    <span itemProp="itemListElement" itemScope itemType="http://schema.org/ListItem">
                        <Link to="/" itemProp="item" style={{ marginRight: "5px", marginLeft: "5px" }}>
                            <span itemProp="name">
                                {"Home"}
                            </span>
                        </Link>
                        <meta itemProp="position" content="1" />
                    </span>
                    {" > "}
                    <span itemProp="itemListElement" itemScope itemType="http://schema.org/ListItem">
                        <span itemProp="name" style={{ marginRight: "5px", marginLeft: "5px" }}>
                            {category}
                        </span>
                        <meta itemProp="position" content="2" />
                    </span>
                </div>
                <hr />
                <h1>{category}</h1>
                <br />
                {lineChangeDesc}
                <br />
                <hr />
                <h2>Keywords about {category}</h2>
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th>Keywords</th>
                            <th>Found Articles</th>
                        </tr>
                    </thead>
                    <tbody>
                        {pages.length > 0 ? pages.filter(page => page.referenceCount > 4).map(page =>
                            <tr key={page.wordId}>
                                <td>
                                    <Link to={"/word/" + page.wordId}>{page.word}</Link>
                                </td>
                                <td>
                                    {page.referenceCount} pages
                                </td>
                            </tr>
                        )
                            :
                            <tr><td>Loading...</td><td></td></tr>}
                    </tbody>
                </table>
            </div>
        );
    }
}

function renderTable(props) {
    const { pages, wordId } = props.pages;
    return (
        <table className='table table-striped'>
            <thead>
                <tr>
                    <th>Page Title</th>
                    <th>Snippet</th>
                </tr>
            </thead>
            <tbody>
                {pages && pages.length > 0 ? pages.map((page, i) => (
                    <tr key={i}>
                        <td>
                            {page.wordId !== wordId && page.referenceCount > 4 ? <Link to={"/word/" + page.wordId}>{page.word}</Link> : page.word}
                        </td>
                        <td>
                            {page.snippet.split(" ").map((s, j) => {
                                const patterns = {
                                    '&lt;': '<',
                                    '&gt;': '>',
                                    '&amp;': '&',
                                    '&quot;': '"',
                                    '&#x27;': '\'',
                                    '&#x60;': '`'
                                };
                                Object.keys(patterns).forEach(k => { s = s.split(k).join(patterns[k]) });
                                const symbol = j === 0 ? "" : " ";
                                const words = props.pages.word.split(" ");
                                if (words.some(w => w.toLowerCase() === s.toLowerCase())) {
                                    return <span key={j}>{symbol}<b>{s}</b></span>;
                                } else if (words.some(w => (w.toLowerCase() + ",") === s.toLowerCase())) {
                                    return <span key={j}>{symbol}<b>{s.slice(0, -1)}</b>,</span>;
                                } else if (words.some(w => (w.toLowerCase() + ",\"") === s.toLowerCase())) {
                                    return <span key={j}>{symbol}<b>{s.slice(0, -1)}</b>{",\""}</span>;
                                } else if (words.some(w => (w.toLowerCase() + ".") === s.toLowerCase())) {
                                    return <span key={j}>{symbol}<b>{s.slice(0, -1)}</b>.</span>;
                                } else if (words.some(w => (w.toLowerCase() + ")") === s.toLowerCase())) {
                                    return <span key={j}>{symbol}<b>{s.slice(0, -1)}</b>{")"}</span>;
                                } else if (words.some(w => (w.toLowerCase() + "\"") === s.toLowerCase())) {
                                    return <span key={j}>{symbol}<b>{s.slice(0, -1)}</b>{"\""}</span>;
                                } else if (words.some(w => ("(" + w.toLowerCase()) === s.toLowerCase())) {
                                    return <span key={j}>{symbol}{"("}<b>{s.substr(1)}</b></span>;
                                } else if (words.some(w => ("\"" + w.toLowerCase()) === s.toLowerCase())) {
                                    return <span key={j}>{symbol}{"\""}<b>{s.substr(1)}</b></span>;
                                } else if (words.some(w => ("\"\"" + w.toLowerCase()) === s.toLowerCase())) {
                                    return <span key={j}>{symbol}{"\"\""}<b>{s.substr(1)}</b></span>;
                                } else {
                                    return <span key={j}>{symbol}{s}</span>;
                                }
                            })}
                            <br />
                            <a href={"https://en.wikipedia.org/wiki/" + page.word.split(" ").join("_")} target="_blank" rel="noopener noreferrer">
                                {"Check the Wikipedia page for " + page.word + " >>"}
                            </a>
                        </td>
                    </tr>
                ))
                    :
                    <tr><td>Loading...</td><td></td></tr>}
            </tbody>
        </table>
    );
}

class RenderOtherTable extends Component {

    constructor(props) {
        super(props);

        this.state = {
            pages: {},
        }
    }

    componentDidMount() {
        const getData = async () => {
            const url = `api/WikiWalks/getWordsForCategory?category=${this.props.c}`;
            const response = await fetch(url);
            const pages = await response.json();
            this.setState({ pages });
        }
        getData();
    }

    render() {
        const { pages } = this.state;
        const { c, wordId } = this.props;
        return (<div>
            <h2>{c}</h2>
            <table className='table table-striped'>
                <thead>
                    <tr>
                        <th>Page Title</th>
                        <th>Snippet</th>
                    </tr>
                </thead>
                <tbody>
                    {pages.length > 0 ? pages.map(page =>
                        <tr key={page.wordId}>
                            <td>
                                {page.wordId !== wordId && page.referenceCount > 4 ? <Link to={"/word/" + page.wordId}>{page.word}</Link> : page.word}
                            </td>
                            <td>
                                {page.snippet}
                                <br />
                                <a href={"https://en.wikipedia.org/wiki/" + page.word.split(" ").join("_")} target="_blank" rel="noopener noreferrer">
                                    {"Check the Wikipedia page for " + page.word + " >>"}
                                </a>
                            </td>
                        </tr>
                    )
                        :
                        <tr><td>Loading...</td><td></td></tr>}
                </tbody>
            </table>
        </div>);
    }
}

export default connect(
    state => state.wikiWalks,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Category);
