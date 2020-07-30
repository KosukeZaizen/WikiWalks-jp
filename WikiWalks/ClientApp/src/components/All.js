import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { actionCreators } from '../store/WikiWalks';
import Head from './Helmet';

class Category extends Component {

    unmounted = false;

    sectionStyle = {
        display: "block",
        borderTop: "1px solid #dcdcdc",
        paddingTop: 12,
        marginTop: 12,
    };

    constructor(props) {
        super(props);

        this.state = {
            pages: [],
        }
    }

    componentDidMount() {
        const getData = async () => {

            let moreData = true;
            let i = 100;
            while (moreData) {
                const url = `api/WikiWalks/getPartialWords?num=${i}`;
                const response = await fetch(url);
                const pages = await response.json();

                if (this.unmounted) { break; }

                this.setState({ pages });

                await new Promise(resolve => setTimeout(() => resolve(), 100));
                i = i + 10;
            }
        }
        getData();
    }

    componentWillUnmount() {
        this.unmounted = true;
    }

    render() {
        const { pages } = this.state;
        const description = `Wikipedia記事に含まれるキーワードの一覧です。各キーワード毎に、最大500記事のWikipediaページをご紹介します。`;
        const arrDesc = description.split("。");
        const lineChangeDesc = arrDesc.map((d, i) => <span key={i}>{d}{i < arrDesc.length - 1 && "。"}<br /></span>);
        return (
            <div>
                <Head
                    title={"キーワード一覧"}
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
                            {"キーワード一覧"}
                        </span>
                        <meta itemProp="position" content="2" />
                    </span>
                </div>
                <section style={this.sectionStyle}>
                    <h1>キーワード一覧</h1>
                    <br />
                    {lineChangeDesc}
                    <table className='table table-striped'>
                        <thead>
                            <tr>
                                <th>キーワード</th>
                                <th><span style={{
                                    display: "inline-block",
                                    minWidth: 70,
                                }}>記事数</span></th>
                            </tr>
                        </thead>
                        <tbody>
                            {pages.length > 0 ? pages.filter(page => page.referenceCount > 4).map(page =>
                                <tr key={page.wordId}>
                                    <td>
                                        <Link to={"/word/" + page.wordId}>{page.word}</Link>
                                    </td>
                                    <td>
                                        {page.referenceCount} 記事
                                </td>
                                </tr>
                            )
                                :
                                <tr><td>Loading...</td><td></td></tr>}
                        </tbody>
                    </table>
                </section>
            </div>
        );
    }
}

export default connect(
    state => state.wikiWalks,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Category);
