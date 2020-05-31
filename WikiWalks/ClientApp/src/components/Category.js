import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { actionCreators } from '../store/WikiWalks';
import Head from './Helmet';

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
            try {
                const url = `api/WikiWalks/getWordsForCategory?category=${this.state.category}`;
                const response = await fetch(url);
                const pages = await response.json();

                if (!pages || pages.length <= 0) window.location.href = `/not-found?p=${window.location.pathname}`;

                this.setState({ pages });
            } catch (e) {
                window.location.href = `/not-found?p=${window.location.pathname}`;
            }
        }
        getData();
    }

    render() {
        const { pages, category } = this.state;
        const description = `This is a list of the keywords about "${category ? category.split("\"").join("") : "..."}". Please check the words below to learn about "${category ? category.split("\"").join("") : "..."}"!`;
        const arrDesc = description.split(". ");
        const lineChangeDesc = arrDesc.map((d, i) => <span key={i}>{d}{i < arrDesc.length - 1 && ". "}<br /></span>);
        return (
            <div>
                <Head
                    title={category}
                    desc={description}
                />
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
                        {pages.length > 0 ? pages.sort((p1, p2) => p2.referenceCount - p1.referenceCount).filter(page => page.referenceCount > 4).map(page =>
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

export default connect(
    state => state.wikiWalks,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Category);